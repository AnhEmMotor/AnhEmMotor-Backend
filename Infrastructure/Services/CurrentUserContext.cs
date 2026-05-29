using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Infrastructure.Services
{
    public class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
    {
        public string GetAuthorizationHeader()
        {
            var httpContext = httpContextAccessor.HttpContext ??
                throw new InvalidOperationException("HTTP context is unavailable.");
            return httpContext.Request.Headers.Authorization.ToString();
        }

        public Guid GetUserId()
        {
            var httpContext = httpContextAccessor.HttpContext ??
                throw new InvalidOperationException("HTTP context is unavailable.");
            var userIdClaim = httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                httpContext.User?.FindFirst("sub")?.Value ??
                throw new UnauthorizedAccessException("User identity claim is missing.");
            return Guid.Parse(userIdClaim);
        }

        public string GetAccessToken()
        {
            var authHeader = GetAuthorizationHeader();
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader[7..].Trim();
            }
            return authHeader;
        }
    }
}