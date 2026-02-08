using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace WebAPI.Extensions;

/// <summary>
///
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(
            options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

        services.AddRateLimiter(
            options =>
            {
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsync("Busy", token).ConfigureAwait(true);
                };

                options.AddFixedWindowLimiter(
                    policyName: "public_api",
                    options =>
                    {
                        options.PermitLimit = 10;
                        options.Window = TimeSpan.FromSeconds(10);
                        options.QueueLimit = 0;
                        options.AutoReplenishment = true;
                    });

                options.AddFixedWindowLimiter(
                    policyName: "auth_api",
                    options =>
                    {
                        options.PermitLimit = 60;
                        options.Window = TimeSpan.FromMinutes(1);
                        options.QueueLimit = 0;
                    });

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                    httpContext =>
                    {
                        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                        return RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: remoteIp,
                            factory: _ => new FixedWindowRateLimiterOptions
                                    {
                                        PermitLimit = 30,
                                        Window = TimeSpan.FromMinutes(1),
                                        QueueLimit = 0
                                    });
                    });
            });

        return services;
    }
}
