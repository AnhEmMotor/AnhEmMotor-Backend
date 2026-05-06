using Application.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý thông báo thời gian thực (SSE)
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý thông báo thời gian thực (SSE)")]
[Route("api/v{version:apiVersion}/[controller]")]
public class NotificationController(INotificationService notificationService) : ApiController
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Đăng ký nhận thông báo thời gian thực cho Admin/Sale
    /// </summary>
    [HttpGet("stream")]
    [Authorize]
    public async Task GetNotificationStreamAsync(CancellationToken cancellationToken)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");
        try
        {
            await Response.WriteAsync(
                $"data: {JsonSerializer.Serialize(new { message = "Connected to notification stream" }, _jsonSerializerOptions)}\n\n",
                cancellationToken)
                .ConfigureAwait(true);
            await Response.Body.FlushAsync(cancellationToken).ConfigureAwait(true);
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await notificationService.WaitForNotificationAsync(cancellationToken).ConfigureAwait(true);
                var notification = new { type = "NewBooking", message, timestamp = DateTimeOffset.UtcNow };
                var json = JsonSerializer.Serialize(notification, _jsonSerializerOptions);
                await Response.WriteAsync($"data: {json}\n\n", cancellationToken).ConfigureAwait(true);
                await Response.Body.FlushAsync(cancellationToken).ConfigureAwait(true);
            }
        } catch (OperationCanceledException)
        {
        }
    }
}
