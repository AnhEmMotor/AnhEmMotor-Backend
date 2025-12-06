using Application.Interfaces.Repositories.Authentication;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services;

/// <summary>
/// Service xử lý tạo JWT Access Token và Refresh Token
/// </summary>
public class TokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager) : ITokenService
{
    /// <summary>
    /// Tạo Access Token cho người dùng
    /// </summary>
    /// <param name="user">Thông tin người dùng</param>
    /// <param name="authMethods">Phương thức xác thực (vd: ["pwd"], ["google"], ["facebook"])</param>
    public async Task<string> CreateAccessTokenAsync(
        ApplicationUser user,
        string[] authMethods,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var userRoles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty),
            new("full_name", user.FullName ?? string.Empty),
            new("status", user.Status ?? string.Empty)
        };

        if(authMethods is { Length: > 0 })
        {
            claims.Add(new Claim("amr", JsonSerializer.Serialize(authMethods), JsonClaimValueTypes.JsonArray));
        }

        foreach(var userRole in userRoles)
        {
            claims.Add(new Claim("role", userRole));
        }

        var jwtKey = configuration["Jwt:Key"];
        var jwtExpiryInMinutes = configuration.GetValue<int>("Jwt:AccessTokenExpiryInMinutes");

        if(string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("Jwt:Key is missing in configuration.");
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var now = DateTimeOffset.UtcNow;
        var minutesToAdd = jwtExpiryInMinutes > 0 ? jwtExpiryInMinutes : 15;
        var expiresAt = now.AddMinutes(minutesToAdd);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            expires: expiresAt.UtcDateTime,
            claims: claims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Tạo Refresh Token ngẫu nhiên
    /// </summary>
    public string CreateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public string? GetClaimFromToken(string token, string claimType)
    {
        var jwtKey = configuration["Jwt:Key"];
        if(string.IsNullOrEmpty(jwtKey))
        {
            return null;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtKey);

        try
        {
            tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                },
                out SecurityToken validatedToken);

            if(validatedToken is JwtSecurityToken jwtToken)
            {
                return jwtToken.Claims.FirstOrDefault(c => string.Compare(c.Type, claimType) == 0)?.Value;
            }
        } catch
        {
        }

        return null;
    }
}
