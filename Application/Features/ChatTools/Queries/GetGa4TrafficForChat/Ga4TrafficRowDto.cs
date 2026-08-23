using System.Text.Json.Serialization;

namespace Application.Features.ChatTools.Queries.GetGa4TrafficForChat;

public sealed record Ga4TrafficRowDto
{
    /// <summary>Nhãn dòng: ngày (yyyy-MM-dd), nguồn traffic, đường dẫn trang, thiết bị hoặc "Tổng".</summary>
    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    [JsonPropertyName("sessions")]
    public long Sessions { get; init; }

    [JsonPropertyName("total_users")]
    public long TotalUsers { get; init; }

    [JsonPropertyName("new_users")]
    public long NewUsers { get; init; }

    [JsonPropertyName("page_views")]
    public long PageViews { get; init; }

    [JsonPropertyName("engagement_rate")]
    public double EngagementRate { get; init; }

    [JsonPropertyName("avg_session_duration_seconds")]
    public double AvgSessionDurationSeconds { get; init; }

    [JsonPropertyName("key_events")]
    public double KeyEvents { get; init; }
}
