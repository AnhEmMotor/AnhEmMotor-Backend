using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Features.StoreChat.Commands.RequestHandoff;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Controllers.Base;
using WebAPI.Hubs;

namespace WebAPI.Controllers;

/// <summary>
/// Tool ghi DUY NHẤT của persona "store" — sidecar AI gọi khi cần chuyển khách sang nhân viên thật.
/// Tách khỏi PublicChatToolsController (đã khoá đúng 5 tool đọc của Stage 02) để giữ ranh giới rõ ràng
/// giữa tool đọc-an toàn và tool ghi duy nhất được phép ở persona này (chỉ đổi Mode, không chạm dữ
/// liệu nghiệp vụ nào khác). Route vẫn nằm dưới "internal/chat/tools/store/" vì BackendClient
/// (sidecar, Python) hardcode prefix này khi gọi mọi tool công khai.
/// </summary>
[Route("internal/chat/tools/store/handoff")]
[AllowAnonymous]
[WebAPI.Attributes.LocalhostOnly]
[DisableRateLimiting]
public class StoreChatEscalationToolController(
    ISender sender,
    IServerDateProvider dateProvider,
    IHubContext<StoreChatHub> hubContext) : ApiController
{
    [HttpPost("escalate")]
    public async Task<IActionResult> Escalate(
        [FromBody] EscalateToStaffForChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender
            .Send(new RequestHandoffCommand(request.SessionId, null, null, "Ai"), cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return HandleResult(Result<ChatToolEnvelope<EscalateToStaffResultDto>>.Failure(result.Errors));
        }

        await hubContext.Clients.Group(request.SessionId.ToString())
            .SendAsync("ModeChanged", new StoreChatModeChangedPayload(StoreChatMode.Waiting, null), cancellationToken);
        if (result.Value.SystemMessage != null)
        {
            await hubContext.Clients.Group(request.SessionId.ToString())
                .SendAsync("ReceiveMessage", result.Value.SystemMessage, cancellationToken);
        }
        await hubContext.Clients.Group(StoreChatHub.StaffGroupName)
            .SendAsync("SessionUpdated", request.SessionId, cancellationToken);

        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "RequestHandoffCommand",
            new Dictionary<string, string>(),
            null,
            null);
        var envelope = ChatToolEnvelope<EscalateToStaffResultDto>.WrapSingle(new EscalateToStaffResultDto(true), meta);
        return HandleResult(Result<ChatToolEnvelope<EscalateToStaffResultDto>>.Success(envelope));
    }
}

public record EscalateToStaffForChatRequest(Guid SessionId);

public record EscalateToStaffResultDto(bool Escalated);
