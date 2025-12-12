using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebAPI.Extensions;

/// <summary>
/// Provides extension methods for configuring JWT bearer authentication in an ASP.NET Core application.
/// </summary>
/// <remarks>This class contains methods that extend the functionality of IServiceCollection to simplify the setup
/// of JWT authentication using configuration values. It is intended to be used during application startup when
/// configuring authentication services.</remarks>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds JWT bearer authentication to the application's service collection using configuration settings.
    /// </summary>
    /// <remarks>This method configures JWT authentication with validation for issuer, audience, lifetime, and
    /// signing key. The relevant JWT settings must be present in the configuration under the 'Jwt' section. The method
    /// sets the default authentication scheme to JWT bearer.</remarks>
    /// <param name="services">The service collection to which authentication services will be added.</param>
    /// <param name="configuration">The application configuration containing JWT settings such as issuer, audience, and signing key.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the required JWT signing key ('Jwt:Key') is missing from the configuration.</exception>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
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
        });

        return services;
    }
}