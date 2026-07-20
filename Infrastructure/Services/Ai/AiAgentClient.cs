using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Ai;

public interface IAiAgentClient
{
    public Task<AiAgentResponse> ChatSearchAsync(string keyword, string? userId);
    public Task<AiAgentResponse> TestRoleAsync(string? userId, string[] roles);
}

public class AiAgentResponse
{
    public string Result { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class AiAgentClient : IAiAgentClient
{
    private readonly HttpClient _httpClient;
    private readonly IAiSidecarManager _sidecarManager;

    public AiAgentClient(HttpClient httpClient, IAiSidecarManager sidecarManager, IConfiguration config)
    {
        _httpClient = httpClient;
        _sidecarManager = sidecarManager;
        var secret = config["Jwt:Key"] ?? string.Empty;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secret);
    }

    public async Task<AiAgentResponse> ChatSearchAsync(string keyword, string? userId)
    {
        var sidecarUrl = _sidecarManager.SidecarUrl;
        var response = await _httpClient.PostAsJsonAsync($"{sidecarUrl}/search", new { keyword, userId });

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AiAgentResponse>() ??
            throw new Exception("Không nhận được phản hồi từ AI Sidecar.");
    }

    public async Task<AiAgentResponse> TestRoleAsync(string? userId, string[] roles)
    {
        var sidecarUrl = _sidecarManager.SidecarUrl;
        var response = await _httpClient.PostAsJsonAsync($"{sidecarUrl}/test-role", new { userId, roles });

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AiAgentResponse>() ??
            throw new Exception("Không nhận được phản hồi từ AI Sidecar.");
    }
}
