using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Application.Features.ManagerChat.Commands.CreateManagerChatSession;

public class CreateManagerChatSessionCommandHandler(
    IChatInsertRepository chatInsertRepository,
    IPermissionReadRepository permissionReadRepository,
    ICurrentUserContext currentUserContext,
    IAiSidecarUrlProvider sidecarUrlProvider,
    IHttpClientFactory httpClientFactory,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateManagerChatSessionCommand, Result<ChatSession>>
{
    public async Task<Result<ChatSession>> Handle(CreateManagerChatSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.GetUserId();
        bool hasPermission = await permissionReadRepository.HasAnyPermissionAsync(userId, cancellationToken);
        if (!hasPermission)
        {
            return Error.Forbidden();
        }

        var finalTitle = request.Title;
        if (string.IsNullOrWhiteSpace(finalTitle) && !string.IsNullOrWhiteSpace(request.InitialMessage))
        {
            finalTitle = await GenerateTitleFromSidecar(request.InitialMessage, cancellationToken);
        }
        
        if (string.IsNullOrWhiteSpace(finalTitle))
        {
            finalTitle = "New Chat";
        }

        var session = new ChatSession
        {
            Title = finalTitle,
            UserId = userId
        };

        chatInsertRepository.AddSession(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return session;
    }

    private async Task<string> GenerateTitleFromSidecar(string message, CancellationToken cancellationToken)
    {
        try
        {
            var sidecarUrl = sidecarUrlProvider.GetSidecarUrl();
            var client = httpClientFactory.CreateClient();
            
            var requestBody = new { message = message };
            var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync($"{sidecarUrl}/manager-chat/generate-title", requestContent, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var contentString = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<JsonElement>(contentString);
                if (result.TryGetProperty("title", out var titleProp))
                {
                    return titleProp.GetString() ?? string.Empty;
                }
            }
        }
        catch (Exception)
        {
            // fallback
        }
        
        var title = message;
        if (title.Length > 30)
        {
            title = title.Substring(0, 30) + "...";
        }
        return title;
    }
}
