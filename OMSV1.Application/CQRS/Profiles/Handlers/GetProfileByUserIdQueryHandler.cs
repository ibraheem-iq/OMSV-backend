using MediatR;
using OMSV1.Application.Dtos.Profiles;
using OMSV1.Domain.Entities.Profiles;
using OMSV1.Application.Helpers;
using OMSV1.Domain.SeedWork;
using OMSV1.Application.Queries.Profiles;
using OMSV1.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace OMSV1.Application.CQRS.Queries.Profiles
{
    public class GetProfileByUserIdQueryHandler : IRequestHandler<GetProfileByUserIdQuery, ProfileDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;
        public GetProfileByUserIdQueryHandler(IUnitOfWork unitOfWork,AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<ProfileDto> Handle(GetProfileByUserIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Fetch the profile entity with related Governorate and Office
            var profile = await _context.Profiles
                .Include(p => p.Governorate)
                .Include(p => p.Office)
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);// Assuming GetByIdAsync is adjusted to fetch by UserId

                if (profile == null)
                {
                    return null; // Return null or throw an exception as needed
                }

                // Map the entity to DTO
                var profileDto = new ProfileDto
                {
                    ProfileId = profile.Id,
                    FullName = profile.FullName,
                    Position = profile.Position.ToString(),
                    GovernorateName = profile.Governorate?.Name, // Null check to prevent NRE
                    OfficeName = profile.Office?.Name, // Null check to prevent NRE
                    UserId = request.UserId,
                    GovernorateId = profile.Governorate?.Id ?? 0, // Fallback if Governorate is null
                    OfficeId = profile.Office?.Id ?? 0 // Fallback if Office is null
                };

                return profileDto;
            }
            catch (Exception ex)
            {
                // Log the exception (using a logging library like Serilog or NLog is suggested)
                throw new HandlerException("An error occurred while retrieving the profile by user ID.", ex);
            }
        }
    }
}
