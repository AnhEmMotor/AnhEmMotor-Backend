using System.Text;
using System.Text.Json;
using Application.Common.Models;
using Application.Interfaces.Services.Analytics;
using Infrastructure.Configurations.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Analytics;

/// <summary>
/// Forward sự kiện client (Mobile) lên GA4 Measurement Protocol. ApiSecret/MeasurementId chỉ tồn tại ở Backend.
/// Gửi fire-and-forget có timeout ngắn — lỗi tracking không được làm hỏng nghiệp vụ chính.
/// </summary>
public class Ga4MeasurementProtocolService(
    IOptions<GoogleAnalytics4Options> options,
    IHttpClientFactory httpClientFactory,
    ILogger<Ga4MeasurementProtocolService> logger) : IGa4MeasurementProtocolService
{
    private const string CollectUrl = "https://www.google-analytics.com/mp/collect";
    private static readonly JsonSerializerOptions PayloadOptions = new(JsonSerializerDefaults.Web);

    private GoogleAnalytics4Options Options => options.Value;

    public bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(Options.MeasurementId)
            && !string.IsNullOrWhiteSpace(Options.MeasurementProtocolApiSecret);
    }

    public async Task<Result<bool>> SendEventsAsync(
        string clientId,
        string? userId,
        IReadOnlyList<MeasurementProtocolEvent> events,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured())
        {
            return Result<bool>.Failure("Measurement Protocol chưa được cấu hình trên server.");
        }

        if (events.Count == 0)
        {
            return Result<bool>.Success(true);
        }

        try
        {
            var payload = new Dictionary<string, object?>
            {
                ["clientId"] = clientId,
                ["userId"] = string.IsNullOrWhiteSpace(userId) ? null : userId,
                ["nonPersonalizedAds"] = false,
                ["events"] = events.Select(e =>
                {
                    var parameters = new Dictionary<string, object>
                    {
                        ["engagement_time_msec"] = 100
                    };
                    if (e.Timestamp != default)
                    {
                        parameters["timestamp_micros"] = e.Timestamp.ToUnixTimeMilliseconds() * 1000;
                    }

                    if (e.Params is not null)
                    {
                        foreach ((var key, var value) in e.Params)
                        {
                            parameters[key] = value;
                        }
                    }

                    return new Dictionary<string, object?>
                    {
                        ["name"] = SanitizeEventName(e.Name),
                        ["params"] = parameters
                    };
                }).ToArray()
            };

            var client = httpClientFactory.CreateClient("Ga4Analytics");
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"{CollectUrl}?measurement_id={Uri.EscapeDataString(Options.MeasurementId)}&api_secret={Uri.EscapeDataString(Options.MeasurementProtocolApiSecret)}")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload, PayloadOptions), Encoding.UTF8, "application/json")
            };

            using var httpResponse = await client.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            if (!httpResponse.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "GA4 Measurement Protocol trả {StatusCode} khi gửi {Count} sự kiện.",
                    (int)httpResponse.StatusCode,
                    events.Count);
            }

            // MP trả 2xx với body rỗng kể cả khi validation error — không retry để tránh trùng lặp.
            return Result<bool>.Success(httpResponse.IsSuccessStatusCode);
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Không gửi được sự kiện tới GA4 Measurement Protocol.");
            return Result<bool>.Success(false);
        }
    }

    /// <summary>GA4 chỉ nhận event name chữ thường + underscore.</summary>
    private static string SanitizeEventName(string name)
    {
        var normalized = name.Trim().ToLowerInvariant().Replace('-', '_').Replace(' ', '_');
        return normalized.Length > 40 ? normalized[..40] : normalized;
    }
}
