using Application.Features.Notifications.Queries.GetNotificationStream;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý thông báo thời gian thực (SSE)
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý thông báo thời gian thực (SSE)")]
[Route("api/v{version:apiVersion}/[controller]")]
public class NotificationController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Đăng ký nhận thông báo thời gian thực cho Admin/Sale
    /// </summary>
    [HttpGet("stream")]
    [Authorize]
    public async Task<IActionResult> GetNotificationStreamAsync(CancellationToken cancellationToken)
    {
        var stream = await mediator.Send(new GetNotificationStreamQuery(), cancellationToken).ConfigureAwait(false);
        return HandleSseResult(stream);
    }
}
