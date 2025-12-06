using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Authorization;

/// <summary>
/// Attribute that allows only anonymous requests to access an endpoint.
/// If the current request is already authenticated, the request will be rejected.
/// Use this on endpoints like login/register where authenticated users should not call them.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class AnonymousOnlyAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            context.Result = new BadRequestObjectResult(new { Message = "You are already authenticated." });
        }

        return Task.CompletedTask;
    }
}
