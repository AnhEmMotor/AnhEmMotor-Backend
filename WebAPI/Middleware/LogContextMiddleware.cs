using Serilog.Context;

namespace WebAPI.Middleware;

/// <summary>
/// Middleware để đẩy các thông tin bổ sung (như Client IP) vào Serilog LogContext cho mỗi request.
/// </summary>
/// <remarks>
/// Điều này giúp tất cả các log được sinh ra trong quá trình xử lý request (Warning, Error, Business Log) đều tự động
/// có thuộc tính ClientIP mà không cần truyền thủ công.
/// </remarks>
public class LogContextMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Thực thi middleware để đẩy ClientIP vào LogContext.
    /// </summary>
    /// <param name="context">Httpcontext của request hiện tại.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault()
                       ?? context.Connection.RemoteIpAddress?.ToString()
                       ?? "unknown";
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "unknown";
        using (LogContext.PushProperty("ClientIP", clientIp))
            using (LogContext.PushProperty("RequestMethod", method))
                using (LogContext.PushProperty("RequestPath", path))
                {
                    await next(context).ConfigureAwait(true);
                }
    }
}
