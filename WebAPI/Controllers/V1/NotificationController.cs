using Application.Common.Models;
using Application.Features.Notifications.Queries.GetNotificationStream;
using Domain.Primitives;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý thông báo thời gian thực qua Server-Sent Events (SSE) — dành cho Admin và Sale.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý thông báo thời gian thực (SSE)")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class NotificationController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Đăng ký nhận thông báo thời gian thực qua SSE (Server-Sent Events).
    /// Dành cho vai trò Admin và Sale để nhận thông báo real-time về đơn hàng, hợp đồng, sự kiện hệ thống.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>SSE stream với các thông báo thời gian thực.</returns>
    /// <response code="200">Kết nối SSE thành công — stream sẽ gửi sự kiện khi có thông báo mới.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpGet("stream")]
    [Authorize]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotificationStreamAsync(CancellationToken cancellationToken)
    {
        var stream = await mediator.Send(new GetNotificationStreamQuery(), cancellationToken).ConfigureAwait(false);
        return HandleSseResult(stream);
    }
}
