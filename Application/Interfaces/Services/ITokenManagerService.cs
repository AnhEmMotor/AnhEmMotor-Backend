using Application.ApiContracts.Auth.Responses;

namespace Application.Interfaces.Services;

public interface ITokenManagerService
{
    public string CreateAccessToken(UserAuth user, DateTimeOffset expiryTime);

    /// <summary>Ký lại token cũ với hạn mới, giữ nguyên toàn bộ claim (Stage 17.9 — E1).</summary>
    public string RefreshAccessToken(string oldToken, DateTimeOffset expiryTime);

    public string CreateRefreshToken();

    public string? GetClaimFromToken(string token, string claimType);

    public int GetRefreshTokenExpiryDays();

    public int GetAccessTokenExpiryMinutes();

    public string CreateRandomToken(int length = 32);
}
