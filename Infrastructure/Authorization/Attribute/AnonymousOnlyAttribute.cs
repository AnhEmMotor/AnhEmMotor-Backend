using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Infrastructure.Authorization.Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class AnonymousOnlyAttribute : System.Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext?.User;
        if(user?.Identity?.IsAuthenticated == true)
        {
            context.Result = new BadRequestObjectResult(new { Message = "You are already authenticated." });
        }

        return Task.CompletedTask;
    }
}
