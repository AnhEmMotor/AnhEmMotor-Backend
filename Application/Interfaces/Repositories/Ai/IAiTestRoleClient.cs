using Application.ApiContracts.Ai;

namespace Application.Interfaces.Repositories.Ai;

public interface IAiTestRoleClient
{
    public Task<AiAgentResponse<string>> TestRoleAsync(string? userId, string[] roles);
}
