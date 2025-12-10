using Application.ApiContracts.Auth.Requests;

namespace Application.Interfaces.Services;

public interface ITokenManagerService
{
    public Task<string> CreateAccessTokenAsync(
        UserAuthDTO user,
        DateTimeOffset expiryTime,
        CancellationToken cancellationToken);

    public string CreateRefreshToken();

    public string? GetClaimFromToken(string token, string claimType);

    public int GetRefreshTokenExpiryDays();

    public int GetAccessTokenExpiryMinutes();
}
