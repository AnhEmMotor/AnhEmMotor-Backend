using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace Infrastructure.Authorization.Attribute;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class LocalhostOnlyAttribute : System.Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
        if (remoteIp != null && !IPAddress.IsLoopback(remoteIp))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
        return Task.CompletedTask;
    }
}
