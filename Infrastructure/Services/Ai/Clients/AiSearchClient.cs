using Application.ApiContracts.Ai;
using Application.Interfaces.Repositories.Ai;
using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Infrastructure.Services.Ai.Clients;

public class AiSearchClient : IAiSearchClient
{
    private readonly HttpClient _httpClient;
    private readonly IAiSidecarUrlProvider _sidecarUrlProvider;
    private readonly ILogger<AiSearchClient> _logger;

    public AiSearchClient(
        HttpClient httpClient,
        IAiSidecarUrlProvider sidecarUrlProvider,
        IConfiguration config,
        ILogger<AiSearchClient> logger)
    {
        _httpClient = httpClient;
        _sidecarUrlProvider = sidecarUrlProvider;
        _logger = logger;
        var secret = config["Jwt:Key"] ?? string.Empty;
        _httpClient.DefaultRequestHeaders.Add("X-Internal-Secret", secret);
    }

    public async Task<AiAgentResponse<AiSearchResult>> ChatSearchAsync(string keyword, string? userId)
    {
        try
        {
            var url = $"{_sidecarUrlProvider.GetSidecarUrl()}/search";
            _logger.LogInformation("[AiSearchClient] Sending request to: {Url}", url);
            var response = await _httpClient.PostAsJsonAsync(url, new { keyword, userId });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AiAgentResponse<AiSearchResult>>() ??
                throw new Exception("Không nhận được phản hồi từ AI Sidecar.");
        } catch (Exception ex)
        {
            _logger.LogError(ex, "[AiSearchClient] Error during search request.");
            throw;
        }
    }
}
