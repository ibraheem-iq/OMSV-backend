using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OMSV1.Infrastructure.Identity;
using OMSV1.Infrastructure.Persistence;

namespace OMSV1.Infrastructure.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddIdentityCore<ApplicationUser>(opt =>
        {
            opt.Password.RequireNonAlphanumeric = false;
            // Account lockout settings
            opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            opt.Lockout.MaxFailedAccessAttempts = 3;
            opt.Lockout.AllowedForNewUsers = true;
        })
            .AddRoles<AppRole>()
            .AddRoleManager<RoleManager<AppRole>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders(); 

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var tokenKey = config["TokenKey"] ?? throw new Exception("Token Key Not Found");
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    

                };

                options.Events = new JwtBearerEvents 
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if(!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;

                    }
                };
            });
        
        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAdminRole",policy => policy.RequireRole("Admin","SuperAdmin"))
            //.AddPolicy("RequireSupervisorRole", policy => policy.RequireRole("Admin","Supervisor"))
            .AddPolicy("RequireSuperAdminRole", policy => policy.RequireRole("SuperAdmin"));

           // .AddPolicy("RequireDamageDeviceRole", policy => policy.RequireRole("DamageDevice","Supervisor"))
          //  .AddPolicy("RequireDamagePassportRole", policy => policy.RequireRole("DamagePassport","Supervisor"))
            //.AddPolicy("RequireLectureRole", policy => policy.RequireRole("Lecture","Supervisor"))
           // .AddPolicy("RequireAttendanceRole", policy => policy.RequireRole("Attendance","Supervisor"))
            //.AddPolicy("RequireExpenseRole", policy => policy.RequireRole("Expense","Supervisor"));

        


        return services;
    }
}

