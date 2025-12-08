using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class HttpTokenAccessorService(IHttpContextAccessor httpContextAccessor) : IHttpTokenAccessorService
    {
        public string? GetRefreshTokenFromCookie() { return httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"]; }

        public void SetRefreshTokenFromCookie(string token, DateTimeOffset expiresAt)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expiresAt,
                Secure = true,
                SameSite = SameSiteMode.Strict,
            };

            httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        public string? GetAuthorizationValueFromHeader()
        { return httpContextAccessor.HttpContext?.Request.Headers.Authorization; }
    }
}
