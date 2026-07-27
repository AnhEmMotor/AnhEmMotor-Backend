namespace Application.Interfaces.Services;

public interface IChatRunQueue
{
    ValueTask EnqueueAsync(Guid runId, CancellationToken ct = default);
    IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken ct);
}
