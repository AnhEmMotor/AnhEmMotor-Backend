using Application.Interfaces.Services;
using System.Threading.Channels;

namespace Infrastructure.Services.Ai.Runs;

public class ChatRunQueue : IChatRunQueue
{
    private readonly Channel<Guid> _queue;

    public ChatRunQueue()
    {
        var options = new UnboundedChannelOptions { SingleReader = true, SingleWriter = false };
        _queue = Channel.CreateUnbounded<Guid>(options);
    }

    public async ValueTask EnqueueAsync(Guid runId, CancellationToken ct = default)
    {
        await _queue.Writer.WriteAsync(runId, ct);
    }

    public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken ct)
    {
        return _queue.Reader.ReadAllAsync(ct);
    }
}
