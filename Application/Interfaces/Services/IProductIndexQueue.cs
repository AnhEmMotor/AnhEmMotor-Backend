namespace Application.Interfaces.Services;

public interface IProductIndexQueue
{
    public ValueTask EnqueueAsync(int productId, CancellationToken ct = default);

    public IAsyncEnumerable<int> ReadAllAsync(CancellationToken ct);
}
