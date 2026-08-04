using System.Threading.Channels;
using Application.Interfaces.Services;

namespace Infrastructure.Services.Product;

public class ProductIndexQueue : IProductIndexQueue
{
    private readonly Channel<int> _queue;

    public ProductIndexQueue()
    {
        var options = new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        };
        _queue = Channel.CreateUnbounded<int>(options);
    }

    public async ValueTask EnqueueAsync(int productId, CancellationToken ct = default)
    {
        await _queue.Writer.WriteAsync(productId, ct);
    }

    public IAsyncEnumerable<int> ReadAllAsync(CancellationToken ct)
    {
        return _queue.Reader.ReadAllAsync(ct);
    }
}
