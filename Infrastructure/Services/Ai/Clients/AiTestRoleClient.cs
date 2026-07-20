using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.ApiContracts.Ai;
using Application.Interfaces.Ai;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Ai.Clients;

public class AiTestRoleClient : IAiTestRoleClient
{
    private readonly HttpClient _httpClient;
    private readonly IAiSidecarManager _sidecarManager;

    public AiTestRoleClient(HttpClient httpClient, IAiSidecarManager sidecarManager, IConfiguration config)
    {
        _httpClient = httpClient;
        _sidecarManager = sidecarManager;
        var secret = config["Jwt:Key"] ?? string.Empty;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secret);
    }

    public async Task<AiAgentResponse<string>> TestRoleAsync(string? userId, string[] roles)
    {
        var sidecarUrl = _sidecarManager.SidecarUrl;
        var response = await _httpClient.PostAsJsonAsync($"{sidecarUrl}/test-role", new { userId, roles });

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AiAgentResponse<string>>() ??
            throw new Exception("Không nhận được phản hồi từ AI Sidecar.");
    }
}
