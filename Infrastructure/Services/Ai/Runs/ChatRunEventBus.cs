using Application.DTOs.Chat;
using Application.Interfaces.Services;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Infrastructure.Services.Ai.Runs;

public class ChatRunEventBus : IChatRunEventBus
{
    private readonly ConcurrentDictionary<Guid, List<Channel<ChatRunEventDto>>> _subscribers = new();

    public void Publish(Guid runId, ChatRunEventDto evt)
    {
        if (_subscribers.TryGetValue(runId, out var channels))
        {
            lock (channels)
            {
                channels.RemoveAll(ch => ch.Reader.Completion.IsCompleted);
                foreach (var channel in channels)
                {
                    channel.Writer.TryWrite(evt);
                }
            }
        }
    }

    public async IAsyncEnumerable<ChatRunEventDto> SubscribeAsync(
        Guid runId,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var channel = Channel.CreateUnbounded<ChatRunEventDto>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
        var channels = _subscribers.GetOrAdd(runId, _ => new List<Channel<ChatRunEventDto>>());
        lock (channels)
        {
            channels.Add(channel);
        }
        try
        {
            await foreach (var item in channel.Reader.ReadAllAsync(ct))
            {
                yield return item;
            }
        } finally
        {
            lock (channels)
            {
                channels.Remove(channel);
            }
        }
    }
}
