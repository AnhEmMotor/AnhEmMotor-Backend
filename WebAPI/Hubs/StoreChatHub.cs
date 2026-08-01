using System.Threading.RateLimiting;
using Application.Features.StoreChat.Commands.GenerateStoreChatAiReply;
using Application.Features.StoreChat.Commands.SendStoreChatMessage;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace WebAPI.Hubs;

/// <summary>
/// Hub chat công khai cho khách vãng lai trên Store — cho phép anonymous (khác ManagerChatHub), định
/// tuyến theo group-theo-sessionId vì kết nối vô danh không có Context.UserIdentifier.
/// </summary>
public class StoreChatHub(ISender sender, PartitionedRateLimiter<string> messageRateLimiter, ILogger<StoreChatHub> logger) : Hub
{
    public async Task JoinSession(Guid sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId.ToString());
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

        try
        {
            var aiResult = await sender.Send(new GenerateStoreChatAiReplyCommand(sessionId, content));
            if (aiResult.IsSuccess)
            {
                await Clients.Group(sessionId.ToString()).SendAsync("ReceiveMessage", aiResult.Value);
            } else
            {
                logger.LogWarning("Không tạo được phản hồi AI cho phiên {SessionId}: {Error}", sessionId, aiResult.Error?.Message);
            }
        } catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi gọi AI cho phiên {SessionId}", sessionId);
        }
    }
}
