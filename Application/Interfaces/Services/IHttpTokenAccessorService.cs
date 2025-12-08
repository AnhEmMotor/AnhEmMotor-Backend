namespace Application.Interfaces.Services
{
    public interface IHttpTokenAccessorService
    {
        string? GetRefreshTokenFromCookie();
        void SetRefreshTokenFromCookie(string token, DateTimeOffset expiresAt);
        string? GetAuthorizationValueFromHeader();
    }
}
