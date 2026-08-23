using Application.Common.Models;

namespace Application.Interfaces.Services.Analytics;

/// <summary>
/// Gửi sự kiện từ Mobile/ứng dụng client lên GA4 qua Measurement Protocol — api_secret giữ ở Backend,
/// client chỉ POST sự kiện thô tới Backend rồi Backend forward sang Google.
/// </summary>
public interface IGa4MeasurementProtocolService
{
    /// <summary>Measurement Protocol đã cấu hình đủ (Enabled + MeasurementId + ApiSecret).</summary>
    public bool IsConfigured();

    /// <summary>
    /// Forward một batch sự kiện tới GA4 Measurement Protocol.
    /// </summary>
    /// <param name="clientId">ID định danh thiết bị/người dùng do client tự sinh và giữ ổn định.</param>
    /// <param name="userId">Tuỳ chọn: id người dùng đã đăng nhập để GA4 ghép cross-device.</param>
    /// <param name="events">Danh sách sự kiện: name + params phẳng (chỉ kiểu string/number/bool).</param>
    public Task<Result<bool>> SendEventsAsync(
        string clientId,
        string? userId,
        IReadOnlyList<MeasurementProtocolEvent> events,
        CancellationToken cancellationToken);
}

public record MeasurementProtocolEvent(string Name, DateTimeOffset Timestamp, IReadOnlyDictionary<string, object>? Params = null);
