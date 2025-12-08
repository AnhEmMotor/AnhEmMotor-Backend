using Application.ApiContracts.Auth.Requests;

namespace Application.Interfaces.Services;

public interface ITokenManagerService
{
    Task<string> CreateAccessTokenAsync(UserAuthDTO user, DateTimeOffset expiryTime, CancellationToken cancellationToken);
    string CreateRefreshToken();
    string? GetClaimFromToken(string token, string claimType);
    int GetRefreshTokenExpiryDays();
    int GetAccessTokenExpiryMinutes();
}
