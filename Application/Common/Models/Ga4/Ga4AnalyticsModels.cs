using System.Text.Json.Serialization;

namespace Application.Common.Models.Ga4;

/// <summary>Tổng quan chỉ số truy cập GA4 trong một khoảng thời gian.</summary>
public record Ga4OverviewDto
{
    [JsonPropertyName("startDate")]
    public string StartDate { get; init; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; init; } = string.Empty;

    [JsonPropertyName("sessions")]
    public long Sessions { get; init; }

    [JsonPropertyName("totalUsers")]
    public long TotalUsers { get; init; }

    [JsonPropertyName("newUsers")]
    public long NewUsers { get; init; }

    [JsonPropertyName("activeUsers")]
    public long ActiveUsers { get; init; }

    [JsonPropertyName("screenPageViews")]
    public long ScreenPageViews { get; init; }

    /// <summary>Tỷ lệ tương tác (0-1). GA4 đã bỏ bounceRate, dùng engagementRate.</summary>
    [JsonPropertyName("engagementRate")]
    public double EngagementRate { get; init; }

    /// <summary>Thời lượng phiên trung bình (giây).</summary>
    [JsonPropertyName("averageSessionDuration")]
    public double AverageSessionDuration { get; init; }

    /// <summary>Số sự kiện chuyển đổi (key events) trong kỳ.</summary>
    [JsonPropertyName("keyEvents")]
    public double KeyEvents { get; init; }
}

/// <summary>Một dòng dữ liệu GA4 theo chiều phân rã (ngày / nguồn / trang / thiết bị).</summary>
public record Ga4DimensionRowDto
{
    /// <summary>Giá trị chiều: ngày yyyy-MM-dd, nguồn traffic, đường dẫn trang, loại thiết bị...</summary>
    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    [JsonPropertyName("sessions")]
    public long Sessions { get; init; }

    [JsonPropertyName("totalUsers")]
    public long TotalUsers { get; init; }

    [JsonPropertyName("newUsers")]
    public long NewUsers { get; init; }

    [JsonPropertyName("activeUsers")]
    public long ActiveUsers { get; init; }

    [JsonPropertyName("screenPageViews")]
    public long ScreenPageViews { get; init; }

    /// <summary>Tỷ lệ tương tác (0-1) của nhóm này.</summary>
    [JsonPropertyName("engagementRate")]
    public double EngagementRate { get; init; }

    /// <summary>Thời lượng phiên trung bình (giây) của nhóm này.</summary>
    [JsonPropertyName("averageSessionDuration")]
    public double AverageSessionDuration { get; init; }

    [JsonPropertyName("keyEvents")]
    public double KeyEvents { get; init; }
}

/// <summary>Kết quả một report GA4 đã chuẩn hoá.</summary>
public record Ga4ReportDto<T>
{
    [JsonPropertyName("propertyId")]
    public string PropertyId { get; init; } = string.Empty;

    [JsonPropertyName("startDate")]
    public string StartDate { get; init; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; init; } = string.Empty;

    [JsonPropertyName("rows")]
    public IReadOnlyList<T> Rows { get; init; } = [];

    [JsonPropertyName("rowCount")]
    public int RowCount { get; init; }
}

/// <summary>Một dòng số liệu realtime GA4 (30 phút gần nhất): thiết bị / nguồn / phút.</summary>
public record Ga4RealtimeRowDto
{
    /// <summary>Giá trị chiều: loại thiết bị, nguồn traffic, hoặc số phút trước (minutesAgo: 0..29).</summary>
    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    [JsonPropertyName("activeUsers")]
    public long ActiveUsers { get; init; }

    [JsonPropertyName("screenPageViews")]
    public long ScreenPageViews { get; init; }
}

/// <summary>Chỉ số realtime GA4 trong 30 phút qua phục vụ widget dashboard.</summary>
public record Ga4RealtimeDto
{
    [JsonPropertyName("activeUsers")]
    public long ActiveUsers { get; init; }

    [JsonPropertyName("screenPageViews")]
    public long ScreenPageViews { get; init; }

    /// <summary>Người dùng hoạt động theo từng phút trong 30 phút qua.</summary>
    [JsonPropertyName("byMinute")]
    public IReadOnlyList<Ga4RealtimeRowDto> ByMinute { get; init; } = [];

    /// <summary>Thành phần truy cập theo nguồn traffic (sessionSource).</summary>
    [JsonPropertyName("bySource")]
    public IReadOnlyList<Ga4RealtimeRowDto> BySource { get; init; } = [];

    /// <summary>Phân rã theo loại thiết bị.</summary>
    [JsonPropertyName("byDevice")]
    public IReadOnlyList<Ga4RealtimeRowDto> ByDevice { get; init; } = [];

    [JsonPropertyName("retrievedAt")]
    public string RetrievedAt { get; init; } = string.Empty;
}

/// <summary>Yêu cầu chạy report tổng quát lên Google Analytics Data API.</summary>
public record Ga4ReportRequest
{
    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    /// <summary>Tên dimension GA4 (VD: "date", "sessionSource", "pagePath", "deviceCategory"). Trống = tổng cả kỳ.</summary>
    public string? Dimension { get; init; }

    public int Limit { get; init; } = 100;
}

/// <summary>Cấu hình tracking công khai trả về cho các app client — KHÔNG chứa bất kỳ secret nào.</summary>
public record Ga4PublicConfigDto(
    [property: JsonPropertyName("enabled")] bool Enabled,
    [property: JsonPropertyName("storeMeasurementId")] string StoreMeasurementId);

/// <summary>Sự kiện thô do client gửi lên để Backend forward sang Measurement Protocol.</summary>
public record Ga4ClientEvent(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("timestamp")] DateTimeOffset? Timestamp = null,
    [property: JsonPropertyName("params")] IReadOnlyDictionary<string, object>? Params = null);

/// <summary>Payload POST /analytics/events từ Mobile/app client.</summary>
public record Ga4TrackEventsRequest
{
    [JsonPropertyName("clientId")]
    public string ClientId { get; init; } = string.Empty;

    [JsonPropertyName("userId")]
    public string? UserId { get; init; }

    [JsonPropertyName("events")]
    public IReadOnlyList<Ga4ClientEvent> Events { get; init; } = [];
}
