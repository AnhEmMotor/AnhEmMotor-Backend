using Application.Interfaces.Repositories.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration; // Cần thêm namespace này

namespace Infrastructure.Services;

public class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration) : ICurrentUserService 
{
    private readonly IConfiguration _configuration = configuration;

    public string? GetRefreshToken()
    {
        return httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];
    }

    public void SetRefreshToken(string token)
    {
        var expiryDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiryInDays");

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddDays(expiryDays),
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", token, cookieOptions);
    }

    public string? GetAuthorizationHeader()
    {
        return httpContextAccessor.HttpContext?.Request.Headers["Authorization"];
    }
}