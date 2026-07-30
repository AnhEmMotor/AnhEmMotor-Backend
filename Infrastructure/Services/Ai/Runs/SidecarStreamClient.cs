using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Application.Common.Interfaces;
using Application.DTOs.Chat;
using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Ai.Runs;

public class SidecarStreamClient(
    IHttpClientFactory httpClientFactory,
    IAiSidecarUrlProvider sidecarUrlProvider,
    IConfiguration configuration,
    IServerDateProvider dateProvider) : ISidecarStreamClient
{
    public async IAsyncEnumerable<SidecarEvent> StreamAsync(Guid runId, Guid sessionId, string message, string token, [EnumeratorCancellation] CancellationToken ct)
    {
        var sidecarUrl = sidecarUrlProvider.GetSidecarUrl();
        var client = httpClientFactory.CreateClient();

        var requestBody = new
        {
            run_id = runId.ToString(),
            session_id = sessionId.ToString(),
            message = message,
            // Stage 16.2 mục #2 — sidecar KHÔNG được tự tính "hôm nay" theo giờ chạy process (UTC),
            // backend là nguồn duy nhất cho "hôm nay" theo giờ Việt Nam.
            server_date = dateProvider.VietnamNow.ToString("O")
        };

        var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{sidecarUrl}/manager-chat")
        {
            Content = requestContent
        };
        
        var internalSecret = configuration["Jwt:Key"];
        if (!string.IsNullOrEmpty(internalSecret))
        {
            httpRequest.Headers.Add("X-Internal-Secret", internalSecret);
        }

        if (!string.IsNullOrEmpty(token))
        {
            httpRequest.Headers.Add("Authorization", $"Bearer {token}");
        }

        var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var sidecarEvent = JsonSerializer.Deserialize<SidecarEvent>(line, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (sidecarEvent != null)
            {
                yield return sidecarEvent;
            }
        }
    }

    public async Task CancelAsync(Guid runId, CancellationToken ct = default)
    {
        var sidecarUrl = sidecarUrlProvider.GetSidecarUrl();
        var client = httpClientFactory.CreateClient();
        
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{sidecarUrl}/manager-chat/{runId}/cancel");
        
        var internalSecret = configuration["Jwt:Key"];
        if (!string.IsNullOrEmpty(internalSecret))
        {
            httpRequest.Headers.Add("X-Internal-Secret", internalSecret);
        }

        var response = await client.SendAsync(httpRequest, ct);
        response.EnsureSuccessStatusCode();
    }
}
