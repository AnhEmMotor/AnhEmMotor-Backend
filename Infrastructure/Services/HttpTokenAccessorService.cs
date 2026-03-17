using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System;

namespace Infrastructure.Services
{
    public class HttpTokenAccessorService(IHttpContextAccessor httpContextAccessor, Microsoft.Extensions.Configuration.IConfiguration configuration) : IHttpTokenAccessorService
    {
        public string? GetRefreshTokenFromCookie()
        { return httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"]; }

        public void SetRefreshTokenToCookie(string token, DateTimeOffset expiresAt)
        {
            var cookieDomain = configuration["CookieDomain"];

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expiresAt,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            };

            if (!string.IsNullOrEmpty(cookieDomain))
            {
                cookieOptions.Domain = cookieDomain;
            }

            httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        public void DeleteRefreshTokenFromCookie()
        {
            var cookieDomain = configuration["CookieDomain"];

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            };

            if (!string.IsNullOrEmpty(cookieDomain))
            {
                cookieOptions.Domain = cookieDomain;
            }

            httpContextAccessor.HttpContext?.Response.Cookies.Delete("refreshToken", cookieOptions);
        }

        public string? GetAuthorizationValueFromHeader()
        { return httpContextAccessor.HttpContext?.Request.Headers.Authorization; }
    }
}
