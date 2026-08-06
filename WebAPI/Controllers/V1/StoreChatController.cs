using Application.Common.Models;
using Application.Features.StoreChat.Commands.CreateOrRestoreStoreChatSession;
using Application.Features.StoreChat.Commands.LinkStoreChatSessionToCustomer;
using Application.Features.StoreChat.Commands.RequestHandoff;
using Application.Features.StoreChat.Commands.SetStoreChatContactInfo;
using Application.Features.StoreChat.Queries.GetStoreChatHistory;
using Asp.Versioning;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using WebAPI.Hubs;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Chat AI công khai trên Store — khách vãng lai (chưa đăng nhập) chat được, không đụng tới entity/quyền của Manager
/// Chat nội bộ.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Chat AI công khai trên Store")]
[Route("api/v{version:apiVersion}/store-chat")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class StoreChatController(ISender sender, IHubContext<StoreChatHub> hubContext) : ApiController
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
    /// Gắn phiên chat vô danh vào tài khoản khách hàng khi phát hiện đăng nhập giữa chừng. Yêu cầu JWT — CustomerUserId
    /// lấy từ token, không tin dữ liệu client gửi lên.
    /// </summary>
    [HttpPost("sessions/{id:guid}/link-customer")]
    [Authorize]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkToCustomerAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new LinkStoreChatSessionToCustomerCommand(id), cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Khách bấm "Gặp nhân viên" — chuyển phiên vào hàng đợi chờ nhận. Công khai, không cần đăng nhập.
    /// </summary>
    [HttpPost("sessions/{id:guid}/request-handoff")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestHandoffAsync(
        Guid id,
        [FromBody] RequestHandoffRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender
            .Send(
                new RequestHandoffCommand(id, request.ContactName, request.ContactPhone, "Customer"),
                cancellationToken)
            .ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await hubContext.Clients
                .Group(id.ToString())
                .SendAsync(
                    "ModeChanged",
                    new StoreChatModeChangedPayload(StoreChatMode.Waiting, null),
                    cancellationToken);
            await hubContext.Clients
                .Group(StoreChatHub.StaffGroupName)
                .SendAsync("SessionUpdated", id, cancellationToken);
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Khách điền Tên/SĐT trước khi chat (khách vãng lai) — công khai, không cần đăng nhập.
    /// </summary>
    [HttpPost("sessions/{id:guid}/contact-info")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetContactInfoAsync(
        Guid id,
        [FromBody] SetContactInfoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender
            .Send(new SetStoreChatContactInfoCommand(id, request.ContactName, request.ContactPhone), cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }
}

public record RequestHandoffRequest(string? ContactName, string? ContactPhone);

public record SetContactInfoRequest(string ContactName, string ContactPhone);
