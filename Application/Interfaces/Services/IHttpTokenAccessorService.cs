namespace Application.Interfaces.Services
{
    public interface IHttpTokenAccessorService
    {
        string? GetRefreshTokenFromCookie();
        void SetRefreshTokenToCookie(string token, DateTimeOffset expiresAt);
        string? GetAuthorizationValueFromHeader();
    }
}
