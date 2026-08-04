namespace Application.Interfaces.Services;

public interface IChatRunCancellationRegistry
{
    public void Register(Guid runId, CancellationTokenSource cts);

    public void Unregister(Guid runId);

    public bool TryCancel(Guid runId);
}
