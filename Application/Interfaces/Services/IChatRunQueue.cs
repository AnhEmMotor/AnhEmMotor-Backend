namespace Application.Interfaces.Services;

public interface IChatRunQueue
{
    public ValueTask EnqueueAsync(Guid runId, CancellationToken ct = default);

    public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken ct);
}
