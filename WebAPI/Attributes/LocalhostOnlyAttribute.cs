using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace WebAPI.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class LocalhostOnlyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
        
        if (remoteIp != null && !IPAddress.IsLoopback(remoteIp))
        {
            if (remoteIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                remoteIp = remoteIp.MapToIPv4();
            }

            if (!IPAddress.IsLoopback(remoteIp))
            {
                context.Result = new UnauthorizedObjectResult("This endpoint is accessible from localhost only.");
                return;
            }
        }
        
        base.OnActionExecuting(context);
    }
}
