using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System;

namespace Infrastructure.Services
{
    public class HttpTokenAccessorService(IHttpContextAccessor httpContextAccessor) : IHttpTokenAccessorService
    {
        public string? GetRefreshTokenFromCookie()
        { return httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"]; }

        public void SetRefreshTokenToCookie(string token, DateTimeOffset expiresAt)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expiresAt,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/api/v1/auth/refresh-token"
            };

            httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        public string? GetAuthorizationValueFromHeader()
        { return httpContextAccessor.HttpContext?.Request.Headers.Authorization; }
    }
}
