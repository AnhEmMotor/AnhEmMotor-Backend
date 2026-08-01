using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Application.Common.Interfaces;
using Application.DTOs.Chat;
using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Ai;

public class StoreChatAiClient(
    IHttpClientFactory httpClientFactory,
    IAiSidecarUrlProvider sidecarUrlProvider,
    IConfiguration configuration,
    IServerDateProvider dateProvider) : IStoreChatAiClient
{
    public async Task<StoreChatAiReplyResult> GetReplyAsync(
        Guid sessionId,
        string visitorMessage,
        IReadOnlyList<StoreChatHistoryItem> history,
        CancellationToken cancellationToken)
    {
        var sidecarUrl = sidecarUrlProvider.GetSidecarUrl();
        var client = httpClientFactory.CreateClient();

        var requestBody = new
        {
            session_id = sessionId.ToString(),
            message = visitorMessage,
            history = history.Select(h => new { role = h.Role, message = h.Message }),
            server_date = dateProvider.VietnamNow.ToString("O")
        };

        var requestContent = new StringContent(
            JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{sidecarUrl}/store-chat")
        {
            Content = requestContent
        };

        var internalSecret = configuration["Jwt:Key"];
        if (!string.IsNullOrEmpty(internalSecret))
        {
            httpRequest.Headers.Add("X-Internal-Secret", internalSecret);
        }

        var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream);

        var text = new StringBuilder();
        var cardNodes = new List<JsonNode>();
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            var sidecarEvent = JsonSerializer.Deserialize<SidecarEvent>(
                line, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (sidecarEvent == null)
            {
                continue;
            }
            if (sidecarEvent.Type == "text_delta")
            {
                text.Append(sidecarEvent.Payload);
            } else if (sidecarEvent.Type is "product-cards" or "variant-cards")
            {
                var node = JsonNode.Parse(sidecarEvent.Payload);
                if (node != null)
                {
                    node["kind"] = sidecarEvent.Type;
                    cardNodes.Add(node);
                }
            }
        }

        var cardsJson = cardNodes.Count > 0 ? JsonSerializer.Serialize(cardNodes) : null;
        return new StoreChatAiReplyResult(text.ToString(), cardsJson);
    }
}
