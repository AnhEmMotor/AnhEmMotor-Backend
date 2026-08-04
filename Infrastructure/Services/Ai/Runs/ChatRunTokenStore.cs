using Application.Interfaces.Services;
using System.Collections.Concurrent;

namespace Infrastructure.Services.Ai.Runs;

public class ChatRunTokenStore : IChatRunTokenStore
{
    private readonly ConcurrentDictionary<Guid, string> _tokens = new();

    public void Store(Guid runId, string token)
    {
        _tokens[runId] = token;
    }

    public string Take(Guid runId)
    {
        _tokens.TryRemove(runId, out var token);
        return token ?? string.Empty;
    }
}
