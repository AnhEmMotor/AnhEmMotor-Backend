using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Features.ManagerChat.Commands.SendManagerChatMessage;

public class SendManagerChatMessageCommandHandler(
    IChatReadRepository chatReadRepository,
    IChatInsertRepository chatInsertRepository,
    IChatUpdateRepository chatUpdateRepository,
    IPermissionReadRepository permissionReadRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork,
    IHttpClientFactory httpClientFactory,
    IAiSidecarUrlProvider aiSidecarUrlProvider)
    : IRequestHandler<SendManagerChatMessageCommand, Result<ChatMessage>>
{
    public async Task<Result<ChatMessage>> Handle(SendManagerChatMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.GetUserId();
        bool hasPermission = await permissionReadRepository.HasAnyPermissionAsync(userId, cancellationToken);
        if (!hasPermission)
        {
            return Error.Forbidden();
        }

        var session = await chatReadRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session == null || session.UserId != userId)
        {
            return Error.NotFound("Phiên chat không tồn tại hoặc không thuộc quyền sở hữu.");
        }

        // Lưu tin nhắn của người dùng
        var userMessage = new ChatMessage
        {
            SessionId = session.Id,
            Role = "User",
            Message = request.Content
        };
        chatInsertRepository.AddMessage(userMessage);

        session.UpdatedAt = DateTime.UtcNow;
        chatUpdateRepository.UpdateSession(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Gọi AI Sidecar
        var sidecarUrl = aiSidecarUrlProvider.GetSidecarUrl();
        var client = httpClientFactory.CreateClient();
        
        var token = currentUserContext.GetAccessToken();
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var payload = new
        {
            session_id = request.SessionId.ToString(),
            message = request.Content
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync($"{sidecarUrl}/manager-chat", content, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            string aiReply = "Có lỗi xảy ra khi gọi AI Sidecar.";

            if (response.IsSuccessStatusCode)
            {
                var sidecarResponse = JsonSerializer.Deserialize<JsonElement>(responseString);
                if (sidecarResponse.TryGetProperty("reply", out var replyProp))
                {
                    aiReply = replyProp.GetString() ?? string.Empty;
                }
            }

            // Lưu phản hồi từ AI
            var aiMessage = new ChatMessage
            {
                SessionId = session.Id,
                Role = "AI",
                Message = aiReply
            };
            chatInsertRepository.AddMessage(aiMessage);
            
            session.UpdatedAt = DateTime.UtcNow;
            chatUpdateRepository.UpdateSession(session);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return aiMessage;
        }
        catch (Exception)
        {
            var aiMessage = new ChatMessage
            {
                SessionId = session.Id,
                Role = "AI",
                Message = "Không thể kết nối đến AI Sidecar."
            };
            chatInsertRepository.AddMessage(aiMessage);
            
            session.UpdatedAt = DateTime.UtcNow;
            chatUpdateRepository.UpdateSession(session);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return aiMessage;
        }
    }
}
