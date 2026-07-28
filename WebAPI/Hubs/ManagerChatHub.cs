using System.Runtime.CompilerServices;
using Application.Features.ManagerChat.Commands.StreamManagerChatMessage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebAPI.Hubs;

[Authorize]
public class ManagerChatHub(ISender sender) : Hub
{
    public async IAsyncEnumerable<string> SendMessageStream(Guid sessionId, string content, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var userIdStr = Context.UserIdentifier;
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            throw new HubException("Unauthorized");
        }

        var token = Context.GetHttpContext()?.Request.Query["access_token"].ToString();
        if (string.IsNullOrEmpty(token))
        {
            token = Context.GetHttpContext()?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        }

        var command = new StreamManagerChatMessageCommand(sessionId, content, userId, token ?? string.Empty);
        
        var stream = sender.CreateStream(command, cancellationToken);

        await foreach (var chunk in stream)
        {
            yield return chunk;
        }
    }
}
