using System.Runtime.CompilerServices;
using Application.DTOs.Chat;
using Application.Features.ManagerChat.Commands.CancelChatRun;
using Application.Features.ManagerChat.Commands.StartChatRun;
using Application.Features.ManagerChat.Queries.GetChatRunEvents;
using Application.Interfaces.Services;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebAPI.Hubs;

[Authorize]
public class ManagerChatHub(ISender sender, IChatRunEventBus bus) : Hub
{
    public async Task<Guid> StartRun(Guid sessionId, string content)
    {
        var userId = ParseUserId();
        var token = ExtractToken();
        var command = new StartChatRunCommand(sessionId, content, userId, token);
        return await sender.Send(command);
    }

    public async IAsyncEnumerable<ChatRunEventDto> SubscribeRun(
        Guid runId, long afterSeq, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Subscribe trước để không lọt event xảy ra giữa lúc replay và lúc nối bus.
        var bufferCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var liveEvents = bus.SubscribeAsync(runId, bufferCts.Token);
        var liveEnumerator = liveEvents.GetAsyncEnumerator(bufferCts.Token);

        var query = new GetChatRunEventsQuery(runId, afterSeq);
        var result = await sender.Send(query, cancellationToken);
        if (result.IsFailure)
        {
            await liveEnumerator.DisposeAsync();
            throw new HubException(result.Error!.Message);
        }

        long lastSeq = afterSeq;
        foreach (var e in result.Value!.Events)
        {
            lastSeq = e.Seq;
            yield return e;
        }

        if (result.Value.RunIsTerminal)
        {
            await liveEnumerator.DisposeAsync();
            yield break;
        }

        try
        {
            while (await liveEnumerator.MoveNextAsync())
            {
                var e = liveEnumerator.Current;
                if (e.Seq <= lastSeq) continue;
                lastSeq = e.Seq;
                yield return e;
                if (IsTerminal(e.Type)) yield break;
            }
        }
        finally
        {
            await liveEnumerator.DisposeAsync();
        }
    }

    public async Task CancelRun(Guid runId)
    {
        var userId = ParseUserId();
        await sender.Send(new CancelChatRunCommand(runId, userId));
    }

    private Guid ParseUserId()
    {
        if (!Guid.TryParse(Context.UserIdentifier, out var userId))
        {
            throw new HubException("Unauthorized");
        }
        return userId;
    }

    private string ExtractToken()
    {
        var token = Context.GetHttpContext()?.Request.Query["access_token"].ToString();
        if (string.IsNullOrEmpty(token))
        {
            token = Context.GetHttpContext()?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        }
        return token ?? string.Empty;
    }

    private static bool IsTerminal(string type) =>
        type is ChatRunEventType.RunCompleted or ChatRunEventType.RunCancelled or ChatRunEventType.Error;
}
