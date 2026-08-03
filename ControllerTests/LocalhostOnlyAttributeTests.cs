using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using WebAPI.Attributes;

namespace ControllerTests;

public class LocalhostOnlyAttributeTests
{
    private static ActionExecutingContext BuildContext(IPAddress? remoteIp)
    {
        var httpContext = new DefaultHttpContext
        {
            Connection = { RemoteIpAddress = remoteIp }
        };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: new object());
    }

    [Theory(DisplayName = "LOCALHOSTONLY_01 - Chặn request không đến từ localhost")]
    [InlineData("8.8.8.8")]
    [InlineData("203.0.113.5")]
    public void OnActionExecuting_ChanIpNgoai(string ip)
    {
        var context = BuildContext(IPAddress.Parse(ip));

        new LocalhostOnlyAttribute().OnActionExecuting(context);

        context.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Theory(DisplayName = "LOCALHOSTONLY_02 - Cho phép request từ localhost")]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    public void OnActionExecuting_ChoPhepLoopback(string ip)
    {
        var context = BuildContext(IPAddress.Parse(ip));

        new LocalhostOnlyAttribute().OnActionExecuting(context);

        context.Result.Should().BeNull();
    }
}
