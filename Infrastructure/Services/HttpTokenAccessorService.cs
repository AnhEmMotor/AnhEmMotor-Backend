using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;

namespace Infrastructure.Services
{
    public class HttpTokenAccessorService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : IHttpTokenAccessorService
    {
        public string? GetRefreshTokenFromCookie()
        { return httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"]; }

        public void SetRefreshTokenToCookie(string token, DateTimeOffset expiresAt)
        {
            var cookieDomain = configuration["CookieDomain"];

            var isHttps = httpContextAccessor.HttpContext?.Request.IsHttps ?? false;
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expiresAt,
                Secure = isHttps,
                SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Path = "/"
            };

            if(!string.IsNullOrEmpty(cookieDomain))
            {
                cookieOptions.Domain = cookieDomain;
            }

            httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        public void DeleteRefreshTokenFromCookie()
        {
            var cookieDomain = configuration["CookieDomain"];

            var isHttps = httpContextAccessor.HttpContext?.Request.IsHttps ?? false;
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Path = "/"
            };

            if(!string.IsNullOrEmpty(cookieDomain))
            {
                cookieOptions.Domain = cookieDomain;
            }

            httpContextAccessor.HttpContext?.Response.Cookies.Delete("refreshToken", cookieOptions);
        }

        public string? GetAuthorizationValueFromHeader()
        { return httpContextAccessor.HttpContext?.Request.Headers.Authorization; }
    }
}
