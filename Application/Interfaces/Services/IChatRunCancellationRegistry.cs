namespace Application.Interfaces.Services;

public interface IChatRunCancellationRegistry
{
    void Register(Guid runId, CancellationTokenSource cts);
    void Unregister(Guid runId);
    bool TryCancel(Guid runId);
}
