using Application.ApiContracts.Ai;

namespace Application.Interfaces.Ai;

public interface IAiSearchClient
{
    public Task<AiAgentResponse<AiSearchResult>> ChatSearchAsync(string keyword, string? userId);
}
