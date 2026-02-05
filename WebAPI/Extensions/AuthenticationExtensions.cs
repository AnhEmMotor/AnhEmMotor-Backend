using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection; // For GetRequiredService
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace WebAPI.Extensions;

/// <summary>
/// Provides extension methods for configuring JWT bearer authentication in an ASP.NET Core application.
/// </summary>
/// <remarks>
/// This class contains methods that extend the functionality of IServiceCollection to simplify the setup of JWT
/// authentication using configuration values. It is intended to be used during application startup when configuring
/// authentication services.
/// </remarks>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds JWT bearer authentication to the application's service collection using configuration settings.
    /// </summary>
    /// <remarks>
    /// This method configures JWT authentication with validation for issuer, audience, lifetime, and signing key. The
    /// relevant JWT settings must be present in the configuration under the 'Jwt' section. The method sets the default
    /// authentication scheme to JWT bearer.
    /// </remarks>
    /// <param name="services">The service collection to which authentication services will be added.</param>
    /// <param name="configuration">The application configuration containing JWT settings such as issuer, audience, and signing key.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the required JWT signing key ('Jwt:Key') is missing from the configuration.</exception>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(
            options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(
                options =>
                {
                    var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");

                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        RoleClaimType = "role",
                        NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

                            if (string.IsNullOrEmpty(userId))
                            {
                                context.Fail("Unauthorized");
                                return;
                            }

                            var user = await userManager.FindByIdAsync(userId);
                            if (user == null)
                            {
                                context.Fail("Unauthorized");
                                return;
                            }

                            var tokenSecurityStamp = context.Principal?.FindFirstValue("AspNet.Identity.SecurityStamp");
                            if (tokenSecurityStamp != user.SecurityStamp)
                            {
                                context.Fail("Unauthorized");
                            }
                        }
                    };
                });

        return services;
    }
}