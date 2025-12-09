using Infrastructure.DBContexts; // Namespace chứa DbContext của bạn
using Domain.Entities; // Namespace chứa User/Role
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebAPI.StartupExtensions;

/// <summary>
/// Provides extension methods for configuring custom authentication services, including ASP.NET Core Identity and JWT
/// bearer authentication, in an application's dependency injection container.
/// </summary>
/// <remarks>This class is intended to be used in the application's startup configuration to simplify the setup of
/// authentication and authorization. The extension method configures password requirements, unique email enforcement,
/// and JWT token validation parameters based on the provided configuration. It should be called during service
/// registration to ensure authentication is properly set up before the application starts handling requests.</remarks>
public static class AuthExtensions
{
    /// <summary>
    /// Configures ASP.NET Core Identity and JWT-based authentication for the application using the specified
    /// configuration settings.
    /// </summary>
    /// <remarks>This method sets up password requirements, unique email enforcement, and configures JWT
    /// authentication schemes and token validation parameters. Ensure that the configuration contains valid values for
    /// 'Jwt:Key', 'Jwt:Issuer', and 'Jwt:Audience'.</remarks>
    /// <param name="services">The service collection to which authentication and identity services will be added.</param>
    /// <param name="configuration">The application configuration containing JWT settings such as issuer, audience, and signing key.</param>
    /// <returns>The same service collection instance with authentication and identity services configured.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the JWT signing key is not specified in the configuration.</exception>
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("JWT Key is not configured in appsettings.json.");
        }

        // 1. Identity Config
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDBContext>()
        .AddDefaultTokenProviders();

        // 2. JWT Config
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // Set true nếu chạy Production có HTTPS
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
        });

        return services;
    }
}