using System.Net;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMSV1.Application.CQRS.Users.Commands;
using OMSV1.Application.Dtos.User;
using OMSV1.Application.Helpers;
using OMSV1.Domain.SeedWork;
using OMSV1.Domain.Specifications.Profiles;
using OMSV1.Infrastructure.Identity;
using OMSV1.Infrastructure.Interfaces;

namespace OMSV1.Application.CQRS.Users.Handlers;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, IActionResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IGenericRepository<OMSV1.Domain.Entities.Profiles.Profile> _profileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<AppRole> roleManager,
        ITokenService tokenService,
        IGenericRepository<OMSV1.Domain.Entities.Profiles.Profile> profileRepository,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Basic validation
        if (request == null)
            return ResponseHelper.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid request.");

        if (request.UserId == Guid.Empty)
            return ResponseHelper.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid User ID.");

        if (request.CurrentUser == null)
            return ResponseHelper.CreateErrorResponse(HttpStatusCode.Unauthorized, "Current user information is required.");

        // Find the user to update
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            return ResponseHelper.CreateErrorResponse(HttpStatusCode.NotFound, "User not found.");

        try
        {
            // Get current user's roles for permission checking
            var currentUserRoles = await _userManager.GetRolesAsync(request.CurrentUser);
            bool isSuperAdmin = currentUserRoles.Contains("SuperAdmin");
            bool isAdmin = currentUserRoles.Contains("Admin");

            // Get target user's current roles
            var userCurrentRoles = await _userManager.GetRolesAsync(user);

            // Prevent Admin from modifying SuperAdmin or other Admin users
            if (isAdmin && !isSuperAdmin && 
                (userCurrentRoles.Contains("SuperAdmin") || userCurrentRoles.Contains("Admin")))
            {
                return ResponseHelper.CreateErrorResponse(HttpStatusCode.Forbidden, 
                    "Admins cannot modify SuperAdmin or other Admin users.");
            }

            // Update username if provided
            if (!string.IsNullOrWhiteSpace(request.UserName) && user.UserName != request.UserName)
            {
                var normalizedUsername = request.UserName.ToUpper();
                var existingUser = await _userManager.Users
                    .FirstOrDefaultAsync(x => x.NormalizedUserName == normalizedUsername && x.Id != request.UserId);
                
                if (existingUser != null)
                    return ResponseHelper.CreateErrorResponse(HttpStatusCode.Conflict, 
                        $"Username '{request.UserName}' is already taken.");

                var setUserNameResult = await _userManager.SetUserNameAsync(user, request.UserName);
                if (!setUserNameResult.Succeeded)
                {
                    var errors = setUserNameResult.Errors.Select(e => e.Description);
                    return ResponseHelper.CreateErrorResponse(HttpStatusCode.BadRequest, 
                        "Username update failed", errors);
                }
            }

            // Update roles if provided
            if (request.Roles != null && request.Roles.Any())
            {
                // Validate roles and check permissions
                foreach (var roleName in request.Roles)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                        return ResponseHelper.CreateErrorResponse(HttpStatusCode.BadRequest, 
                            $"Role '{roleName}' does not exist.");

                    // SuperAdmin role restrictions
                    if (roleName == "SuperAdmin" && !isSuperAdmin)
                    {
                        return ResponseHelper.CreateErrorResponse(HttpStatusCode.Forbidden, 
                            "Only a SuperAdmin can assign the SuperAdmin role.");
                    }

                    // Admin role restrictions
                    if (roleName == "Admin" && !isSuperAdmin)
                    {
                        return ResponseHelper.CreateErrorResponse(HttpStatusCode.Forbidden, 
                            "Only a SuperAdmin can assign the Admin role.");
                    }

                    // Prevent Admin from modifying Admin or SuperAdmin roles
                    if (isAdmin && (roleName == "Admin" || roleName == "SuperAdmin"))
                    {
                        return ResponseHelper.CreateErrorResponse(HttpStatusCode.Forbidden, 
                            "Admins cannot assign the Admin or SuperAdmin role.");
                    }
                }

                // Remove existing roles
                if (userCurrentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, userCurrentRoles);
                    if (!removeResult.Succeeded)
                    {
                        var errors = removeResult.Errors.Select(e => e.Description);
                        return ResponseHelper.CreateErrorResponse(HttpStatusCode.BadRequest, 
                            "Failed to update roles", errors);
                    }
                }

                // Add new roles
                var addResult = await _userManager.AddToRolesAsync(user, request.Roles);
                if (!addResult.Succeeded)
                {
                    var errors = addResult.Errors.Select(e => e.Description);
                    return ResponseHelper.CreateErrorResponse(HttpStatusCode.BadRequest, 
                        "Failed to update roles", errors);
                }
            }

            // Update profile if it exists
            var profileSpec = new ProfileByUserIdSpecification(request.UserId);
            var profile = await _profileRepository.SingleOrDefaultAsync(profileSpec);

            if (profile != null)
            {
                profile.UpdateProfile(
                    fullName: request.FullName ?? profile.FullName,
                    position: request.Position,
                    officeId: request.OfficeId ?? profile.OfficeId,
                    governorateId: request.GovernorateId ?? profile.GovernorateId
                );

                await _profileRepository.UpdateAsync(profile);
            }

            // Save all changes
            var saveResult = await _unitOfWork.SaveAsync(cancellationToken);
            if (!saveResult)
                return ResponseHelper.CreateErrorResponse(HttpStatusCode.InternalServerError, 
                    "Failed to save user updates.");

            // Generate new tokens
            var (accessToken, refreshToken, accessTokenExpires, refreshTokenExpires) = 
                await _tokenService.CreateToken(user);

            // Return success response
            return new ObjectResult(new UserDto
            {
                Username = user.UserName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpires = accessTokenExpires,
                RefreshTokenExpires = refreshTokenExpires
            })
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
        catch (Exception ex)
        {
            return ResponseHelper.CreateErrorResponse(HttpStatusCode.InternalServerError, 
                "An unexpected error occurred.", new[] { ex.Message });
        }
    }
}