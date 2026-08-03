using System.Threading.RateLimiting;
using Application.Features.StoreChat.Commands.GenerateStoreChatAiReply;
using Application.Features.StoreChat.Commands.SendStoreChatMessage;
using Application.Features.StoreChat.Commands.SendStoreChatStaffMessage;
using Application.Interfaces.Repositories.StoreChat;
using Domain.Constants;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace WebAPI.Hubs;

/// <summary>
/// Hub chat công khai cho khách vãng lai trên Store — cho phép anonymous (khác ManagerChatHub), định
/// tuyến theo group-theo-sessionId vì kết nối vô danh không có Context.UserIdentifier. Các method dành
/// riêng cho nhân viên (Stage 06) tự gắn [HasPermission] ở method vì class không thể để [Authorize].
/// </summary>
public class StoreChatHub(
    ISender sender,
    IStoreChatReadRepository storeChatReadRepository,
    PartitionedRateLimiter<string> messageRateLimiter,
    ILogger<StoreChatHub> logger) : Hub
{
    /// <summary>Group nhận thông báo phiên đổi trạng thái — trang quản trị Stage 06 join group này.</summary>
    public const string StaffGroupName = "store-chat-staff";

    public async Task JoinSession(Guid sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId.ToString());
    }

    /// <summary>Nhân viên trang quản trị join để nhận SessionUpdated realtime (Stage 06 mục 6.4).</summary>
    [HasPermission(Permissions.Marketing.StoreChatManagement.View)]
    public async Task JoinStaffGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, StaffGroupName);
    }

    /// <summary>
    /// Nhân viên gửi tin nhắn trực tiếp cho khách — tự nhận phiên (Ai/Waiting -> Human) ngay tại đây,
    /// không cần bước "Nhận" riêng nữa; chặn nếu phiên đang do nhân viên khác phụ trách.
    /// </summary>
    [HasPermission(Permissions.Marketing.StoreChatManagement.Claim)]
    public async Task SendStaffMessage(Guid sessionId, string content, string? cardsJson = null)
    {
        var result = await sender.Send(new SendStoreChatStaffMessageCommand(sessionId, content, cardsJson));
        if (result.IsFailure)
        {
            throw new HubException(result.Error!.Message);
        }
        await Clients.Group(sessionId.ToString()).SendAsync("ReceiveMessage", result.Value.Message);
        await Clients.Group(sessionId.ToString()).SendAsync(
            "ModeChanged", new StoreChatModeChangedPayload(StoreChatMode.Human, result.Value.StaffName));
        await Clients.Group(StaffGroupName).SendAsync("SessionUpdated", sessionId);
    }

    public async Task SendMessage(Guid sessionId, string content)
    {
        var visitorKey = Context.GetHttpContext()?.Request.Query["visitorKey"].ToString();
        if (string.IsNullOrEmpty(visitorKey))
        {
            throw new HubException("Thiếu VisitorKey.");
        }

        using var lease = messageRateLimiter.AttemptAcquire(visitorKey);
        if (!lease.IsAcquired)
        {
            throw new HubException("Bạn gửi tin nhắn quá nhanh, vui lòng thử lại sau.");
        }

        var result = await sender.Send(new SendStoreChatMessageCommand(sessionId, content));
        if (result.IsFailure)
        {
            throw new HubException(result.Error!.Message);
        }
        await Clients.Group(sessionId.ToString()).SendAsync("ReceiveMessage", result.Value);
        await Clients.Group(StaffGroupName).SendAsync("SessionUpdated", sessionId);

        var session = await storeChatReadRepository.GetSessionByIdAsync(sessionId);
        if (session == null || session.Mode != StoreChatMode.Ai)
        {
            // Waiting/Human: tin nhắn đã lưu + phát ở trên là đủ, AI không trả lời cho tới khi nhân viên trả lại.
            return;
        }

        try
        {
            await Clients.Group(sessionId.ToString()).SendAsync("AiTyping");
            var aiResult = await sender.Send(new GenerateStoreChatAiReplyCommand(sessionId, content, OnChunk: async delta =>
            {
                try
                {
                    await Clients.Group(sessionId.ToString()).SendAsync("ReceiveMessageChunk", delta);
                } catch
                {
                    // Khách đã rời tab giữa chừng — không được làm hỏng việc sinh + lưu câu trả lời AI vào DB.
                }
            }));
            if (aiResult.IsSuccess && aiResult.Value != null)
            {
                await Clients.Group(sessionId.ToString()).SendAsync("ReceiveMessage", aiResult.Value);
            } else if (aiResult.IsFailure)
            {
                logger.LogWarning("Không tạo được phản hồi AI cho phiên {SessionId}: {Error}", sessionId, aiResult.Error?.Message);
            }
        } catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi gọi AI cho phiên {SessionId}", sessionId);
        }
    }
}

/// <summary>Payload thống nhất cho event "ModeChanged" — StaffName null khi không phải Human.</summary>
public record StoreChatModeChangedPayload(string Mode, string? StaffName);
