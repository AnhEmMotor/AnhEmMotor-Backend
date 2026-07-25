using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Features.ManagerChat.Commands.StreamManagerChatMessage;

public class StreamManagerChatMessageCommandHandler(
    IChatReadRepository chatReadRepository,
    IChatInsertRepository chatInsertRepository,
    IPermissionReadRepository permissionReadRepository,
    IAiSidecarUrlProvider sidecarUrlProvider,
    IUnitOfWork unitOfWork,
    IHttpClientFactory httpClientFactory) : IStreamRequestHandler<StreamManagerChatMessageCommand, string>
{
    public async IAsyncEnumerable<string> Handle(StreamManagerChatMessageCommand request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        bool hasPermission = await permissionReadRepository.HasAnyPermissionAsync(request.UserId, cancellationToken);
        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("Forbidden");
        }

        var session = await chatReadRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session == null || session.UserId != request.UserId)
        {
            throw new InvalidOperationException("Phiên chat không tồn tại hoặc không thuộc quyền sở hữu.");
        }

        var userMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            Role = "User",
            Message = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        chatInsertRepository.AddMessage(userMessage);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var sidecarUrl = sidecarUrlProvider.GetSidecarUrl();
        var client = httpClientFactory.CreateClient();
        
        var requestBody = new
        {
            session_id = request.SessionId.ToString(),
            message = request.Content
        };

        var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{sidecarUrl}/manager-chat")
        {
            Content = requestContent
        };
        
        if (!string.IsNullOrEmpty(request.Token))
        {
            httpRequest.Headers.Add("Authorization", $"Bearer {request.Token}");
        }

        var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var fullReply = new StringBuilder();
        var buffer = new char[32]; // Read in small chunks for streaming effect
        int charsRead;

        while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var chunk = new string(buffer, 0, charsRead);
            fullReply.Append(chunk);
            yield return chunk;
        }

        var aiMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            Role = "AI",
            Message = fullReply.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        chatInsertRepository.AddMessage(aiMessage);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
