using Application.ApiContracts.Auth.Responses;
using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services;

public class TokenManagerService : ITokenManagerService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SymmetricSecurityKey _authSigningKey;
    private readonly int _accessTokenExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;

    public TokenManagerService(IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"];
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var accessTokenExpiry = configuration["Jwt:AccessTokenExpiryInMinutes"];
        var refreshTokenExpiry = configuration["Jwt:RefreshTokenExpiryInDays"];

        if(string.IsNullOrEmpty(jwtKey))
            throw new InvalidOperationException("Jwt:Key is missing.");
        if(string.IsNullOrEmpty(issuer))
            throw new InvalidOperationException("Jwt:Issuer is missing.");
        if(string.IsNullOrEmpty(audience))
            throw new InvalidOperationException("Jwt:Audience is missing.");

        _authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        _issuer = issuer;
        _audience = audience;

        _accessTokenExpiryMinutes = int.TryParse(accessTokenExpiry, out var accessMinutes) && accessMinutes > 0
            ? accessMinutes
            : 15;
        _refreshTokenExpiryDays = int.TryParse(refreshTokenExpiry, out var refreshDays) && refreshDays > 0
            ? refreshDays
            : 7;
    }

    public string CreateAccessToken(UserAuth user, DateTimeOffset expiryTime)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty),
            new(Domain.Constants.ClaimJWTPayload.FullName, user.FullName ?? string.Empty),
            new(Domain.Constants.ClaimJWTPayload.Status, user.Status ?? string.Empty),
            new("AspNet.Identity.SecurityStamp", user.SecurityStamp ?? string.Empty)
        };

        if(user.AuthMethods is { Length: > 0 })
        {
            claims.Add(
                new Claim(
                    Domain.Constants.ClaimJWTPayload.Amr,
                    JsonSerializer.Serialize(user.AuthMethods),
                    JsonClaimValueTypes.JsonArray));
        }

        if(user.Roles is not null)
        {
            foreach(var role in user.Roles)
            {
                claims.Add(new Claim(Domain.Constants.ClaimJWTPayload.Role, role));
            }
        }

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            expires: expiryTime.UtcDateTime,
            claims: claims,
            signingCredentials: new SigningCredentials(_authSigningKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
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
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _authSigningKey,
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
            return null;
        }

        return null;
    }

    public int GetRefreshTokenExpiryDays() => _refreshTokenExpiryDays;

    public int GetAccessTokenExpiryMinutes() => _accessTokenExpiryMinutes;
}