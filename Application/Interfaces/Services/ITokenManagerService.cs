using Application.ApiContracts.Auth.Requests;

namespace Application.Interfaces.Services;

public interface ITokenManagerService
{
    public string CreateAccessToken(UserAuthDTO user, DateTimeOffset expiryTime);
    public string CreateRefreshToken();
    public string? GetClaimFromToken(string token, string claimType);
    public int GetRefreshTokenExpiryDays();
    public int GetAccessTokenExpiryMinutes();
}