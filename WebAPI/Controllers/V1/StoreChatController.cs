using Application.Common.Models;
using Application.Features.StoreChat.Commands.CreateOrRestoreStoreChatSession;
using Application.Features.StoreChat.Commands.LinkStoreChatSessionToCustomer;
using Application.Features.StoreChat.Queries.GetStoreChatHistory;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Chat AI công khai trên Store — khách vãng lai (chưa đăng nhập) chat được, không đụng tới entity/quyền
/// của Manager Chat nội bộ.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Chat AI công khai trên Store")]
[Route("api/v{version:apiVersion}/store-chat")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class StoreChatController(ISender sender) : ApiController
{
    /// <summary>
    /// Tạo phiên chat mới hoặc khôi phục phiên cũ theo VisitorKey — khách không cần đăng nhập.
    /// </summary>
    [HttpPost("sessions")]
    [AllowAnonymous]
    [EnableRateLimiting("store_chat_session_creation")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrRestoreSessionAsync(
        CreateOrRestoreStoreChatSessionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy lịch sử tin nhắn của một phiên chat — công khai, không cần đăng nhập.
    /// </summary>
    [HttpGet("sessions/{id:guid}/history")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoreChatHistoryQuery(id), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Gắn phiên chat vô danh vào tài khoản khách hàng khi phát hiện đăng nhập giữa chừng. Yêu cầu JWT —
    /// CustomerUserId lấy từ token, không tin dữ liệu client gửi lên.
    /// </summary>
    [HttpPost("sessions/{id:guid}/link-customer")]
    [Authorize]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkToCustomerAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new LinkStoreChatSessionToCustomerCommand(id), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
