using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System.Threading.RateLimiting;

namespace WebAPI.Extensions;

/// <summary>
/// Provides extension methods for configuring rate limiting services.
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Adds rate limiting services to the specified <see cref="IServiceCollection"/>. Loopback (localhost) requests are
    /// excluded from rate limiting.
    /// </summary>
    /// <param name="services">The service collection to add rate limiting to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
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

                options.AddPolicy(
                    policyName: "public_api",
                    partitioner: httpContext =>
                    {
                        if(IsLoopback(httpContext))
                        {
                            return RateLimitPartition.GetNoLimiter(string.Empty);
                        }

                        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                        return RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: remoteIp,
                            factory: _ => new FixedWindowRateLimiterOptions
                                    {
                                        PermitLimit = 5,
                                        Window = TimeSpan.FromSeconds(1),
                                        QueueLimit = 0,
                                        AutoReplenishment = true
                                    });
                    });

                options.AddPolicy(
                    policyName: "auth_api",
                    partitioner: httpContext =>
                    {
                        if(IsLoopback(httpContext))
                        {
                            return RateLimitPartition.GetNoLimiter(string.Empty);
                        }

                        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                        return RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: remoteIp,
                            factory: _ => new FixedWindowRateLimiterOptions
                                    {
                                        PermitLimit = 50,
                                        Window = TimeSpan.FromSeconds(1),
                                        QueueLimit = 0
                                    });
                    });

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                    httpContext =>
                    {
                        if(IsLoopback(httpContext))
                        {
                            return RateLimitPartition.GetNoLimiter(string.Empty);
                        }

                        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                        return RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: remoteIp,
                            factory: _ => new FixedWindowRateLimiterOptions
                                    {
                                        PermitLimit = 200,
                                        Window = TimeSpan.FromSeconds(1),
                                        QueueLimit = 0
                                    });
                    });
            });

        return services;
    }

    private static bool IsLoopback(HttpContext httpContext)
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress;

        return remoteIp is not null && IPAddress.IsLoopback(remoteIp);
    }
}
