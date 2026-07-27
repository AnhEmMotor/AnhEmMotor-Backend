using System.Threading.Channels;
using Application.Interfaces.Services;

namespace Infrastructure.Services.Ai.Runs;

public class ChatRunQueue : IChatRunQueue
{
    private readonly Channel<Guid> _queue;

    public ChatRunQueue()
    {
        var options = new UnboundedChannelOptions
        {
            SingleReader = true, // ChatRunExecutor will read
            SingleWriter = false // Multiple commands can write
        };
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
