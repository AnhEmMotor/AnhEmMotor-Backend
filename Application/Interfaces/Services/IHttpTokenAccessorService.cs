namespace Application.Interfaces.Services
{
    public interface IHttpTokenAccessorService
    {
        public string? GetRefreshTokenFromCookie();

        public void SetRefreshTokenToCookie(string token, DateTimeOffset expiresAt);

        public void DeleteRefreshTokenFromCookie();

        public string? GetAuthorizationValueFromHeader();
    }
}
