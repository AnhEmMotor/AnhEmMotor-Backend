using System.Collections.Concurrent;
using Application.Interfaces.Services;

namespace Infrastructure.Services.Ai.Runs;

// ponytail: in-memory token handoff, tied to a single instance — fine while runs stay single-instance (Stage 8 scope)
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
