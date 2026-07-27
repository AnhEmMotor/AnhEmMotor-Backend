using System.Collections.Concurrent;
using Application.Interfaces.Services;

namespace Infrastructure.Services.Ai.Runs;

public class ChatRunCancellationRegistry : IChatRunCancellationRegistry
{
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _registry = new();

    public void Register(Guid runId, CancellationTokenSource cts)
    {
        _registry.TryAdd(runId, cts);
    }

    public void Unregister(Guid runId)
    {
        if (_registry.TryRemove(runId, out var cts))
        {
            cts.Dispose();
        }
    }

    public bool TryCancel(Guid runId)
    {
        if (_registry.TryGetValue(runId, out var cts))
        {
            try
            {
                cts.Cancel();
                return true;
            }
            catch
            {
                // ignore
            }
        }
        return false;
    }
}
