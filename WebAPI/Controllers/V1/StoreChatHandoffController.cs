using Application.Common.Models;
using Application.Features.StoreChat.Commands.DeleteStoreChatSession;
using Application.Features.StoreChat.Commands.ReleaseStoreChatSession;
using Application.Features.StoreChat.Queries.GetProductVariantsForStaff;
using Application.Features.StoreChat.Queries.GetStoreChatFullHistoryForStaff;
using Application.Features.StoreChat.Queries.GetStoreChatSessionsForStaff;
using Application.Features.StoreChat.Queries.SearchProductsForStaff;
using Asp.Versioning;
using Domain.Constants;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using WebAPI.Hubs;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Trang quản trị phiên chat Store — tách riêng khỏi StoreChatController (công khai) để phân quyền rõ ràng ở tầng
/// route, dễ audit, đúng lý do đã tách AdminChatHistoryController khỏi ManagerChatController.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Trang quản trị phiên chat Store")]
[Route("api/v{version:apiVersion}/store-chat-handoff")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class StoreChatHandoffController(ISender sender, IHubContext<StoreChatHub> hubContext) : ApiController
{
    /// <summary>
    /// Danh sách phiên cho trang quản trị — Stage 06.
    /// </summary>
    [HttpGet("sessions")]
    [HasPermission(Permissions.Marketing.StoreChatManagement.View)]
    public async Task<IActionResult> GetSessionsAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoreChatSessionsForStaffQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lịch sử đầy đủ của 1 phiên cho trang quản trị — không bị lọc bởi mốc "khách đã xoá lịch sử".
    /// </summary>
    [HttpGet("sessions/{id:guid}/history")]
    [HasPermission(Permissions.Marketing.StoreChatManagement.View)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFullHistoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoreChatFullHistoryForStaffQuery(id), cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Nhân viên bấm "Trả lại AI" — chuyển Human -> Ai.
    /// </summary>
    [HttpPost("sessions/{id:guid}/release")]
    [HasPermission(Permissions.Marketing.StoreChatManagement.Claim)]
    public async Task<IActionResult> ReleaseAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ReleaseStoreChatSessionCommand(id), cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await hubContext.Clients
                .Group(id.ToString())
                .SendAsync("ModeChanged", new StoreChatModeChangedPayload(StoreChatMode.Ai, null), cancellationToken);
            await hubContext.Clients
                .Group(StoreChatHub.StaffGroupName)
                .SendAsync("SessionUpdated", id, cancellationToken);
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá 1 phiên chat (kèm toàn bộ tin nhắn) — dùng dọn phiên spam/test. Xoá mềm theo quy ước BaseEntity chung của hệ
    /// thống (set DeletedAt, không xoá vật lý) — ẩn hoàn toàn khỏi trang quản trị, không có UI khôi phục.
    /// </summary>
    [HttpDelete("sessions/{id:guid}")]
    [HasPermission(Permissions.Marketing.StoreChatManagement.Delete)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteStoreChatSessionCommand(id), cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await hubContext.Clients
                .Group(StoreChatHub.StaffGroupName)
                .SendAsync("SessionUpdated", id, cancellationToken);
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Tìm sản phẩm để gán vào tin nhắn gửi khách — cùng quyền với gửi tin (Claim).
    /// </summary>
    [HttpGet("products/search")]
    [HasPermission(Permissions.Marketing.StoreChatManagement.Claim)]
    public async Task<IActionResult> SearchProductsAsync(
        [FromQuery] string? keyword,
        [FromQuery] int limit,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SearchProductsForStaffQuery(keyword, limit == 0 ? 10 : limit),
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Danh sách biến thể + màu sắc của 1 sản phẩm — hiển thị để nhân viên chọn trước khi gán vào tin nhắn.
    /// </summary>
    [HttpGet("products/{id:int}/variants")]
    [HasPermission(Permissions.Marketing.StoreChatManagement.Claim)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductVariantsAsync(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductVariantsForStaffQuery(id), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
