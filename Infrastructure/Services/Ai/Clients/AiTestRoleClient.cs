using Application.ApiContracts.Ai;
using Application.Interfaces.Services;
using Application.Interfaces.Repositories.Ai;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Infrastructure.Services.Ai.Clients;

public class AiTestRoleClient : IAiTestRoleClient
{
    private readonly HttpClient _httpClient;
    private readonly IAiSidecarUrlProvider _sidecarUrlProvider;

    public AiTestRoleClient(HttpClient httpClient, IAiSidecarUrlProvider sidecarUrlProvider, IConfiguration config)
    {
        _httpClient = httpClient;
        _sidecarUrlProvider = sidecarUrlProvider;
        var secret = config["Jwt:Key"] ?? string.Empty;
        _httpClient.DefaultRequestHeaders.Add("X-Internal-Secret", secret);
    }

    public async Task<AiAgentResponse<string>> TestRoleAsync(string? userId, string[] roles)
    {
        var sidecarUrl = _sidecarUrlProvider.GetSidecarUrl();
        var response = await _httpClient.PostAsJsonAsync($"{sidecarUrl}/test-role", new { userId, roles });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AiAgentResponse<string>>() ??
            throw new Exception("Không nhận được phản hồi từ AI Sidecar.");
    }
}
