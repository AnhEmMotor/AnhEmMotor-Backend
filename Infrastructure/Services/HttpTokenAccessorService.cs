using Application.Interfaces.Services;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;

namespace Infrastructure.Services
{
    public class HttpTokenAccessorService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : IHttpTokenAccessorService
    {
        public string? GetRefreshTokenFromCookie()
        {
            return httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];
        }

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
        {
            return httpContextAccessor.HttpContext?.Request.Headers.Authorization;
        }

        public string? GetUserId()
        {
            return httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
        }

        public string? GetAccessToken()
        {
            var authorizationHeader = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return null;
            }
            if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authorizationHeader[7..];
            }
            return authorizationHeader;
        }
    }
}
