using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class CookieTokenManager(IHttpContextAccessor httpContextAccessor, IConfiguration configuration): ICookieTokenManager
    {
        public string? GetRefreshToken()
        {
            return httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];
        }

        public void SetRefreshToken(string token, DateTimeOffset expiresAt)
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

        public void DeleteRefreshToken()
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
    }
}
