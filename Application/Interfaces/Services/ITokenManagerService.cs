using Application.ApiContracts.Auth.Requests;

namespace Application.Interfaces.Services;

public interface ITokenManagerService
{
    string CreateAccessToken(UserAuthDTO user, DateTimeOffset expiryTime);
    string CreateRefreshToken();
    string? GetClaimFromToken(string token, string claimType);
    int GetRefreshTokenExpiryDays();
    int GetAccessTokenExpiryMinutes();
}