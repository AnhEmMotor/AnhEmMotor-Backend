using Application.ApiContracts.Auth.Requests;
using Application.Interfaces.Services;
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

public class TokenManagerService(IConfiguration configuration): ITokenManagerService
{
    public Task<string> CreateAccessTokenAsync(UserAuthDTO user, DateTimeOffset expiryTime, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Name, user.Username ?? string.Empty),
                new("full_name", user.FullName ?? string.Empty),
                new("status", user.Status ?? string.Empty)
            };

        if (user.AuthMethods is { Length: > 0 })
        {
            claims.Add(new Claim("amr", JsonSerializer.Serialize(user.AuthMethods), JsonClaimValueTypes.JsonArray));
        }

        if (user.Roles is null)
        {
            throw new InvalidOperationException("User roles cannot be null.");
        }

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim("role", role));
        }

        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("Jwt:Key is missing in configuration.");
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));


        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            expires: expiryTime.UtcDateTime,
            claims: claims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Task.FromResult(tokenString);
    }

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

    public int GetRefreshTokenExpiryDays()
    {
        var refreshTokenExpiryInMinutes = configuration.GetValue<int>("Jwt:RefreshTokenExpiryInDays");
        return refreshTokenExpiryInMinutes > 0 ? refreshTokenExpiryInMinutes : 7;
    }

    public int GetAccessTokenExpiryMinutes()
    {
        var jwtExpiryInMinutes = configuration.GetValue<int>("Jwt:AccessTokenExpiryInMinutes");
        return jwtExpiryInMinutes > 0 ? jwtExpiryInMinutes : 15;
    }
}
