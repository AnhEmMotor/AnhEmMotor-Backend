namespace Infrastructure.Configurations.Options;

/// <summary>
/// Cấu hình GA4 — CHỈ tồn tại ở Backend. Điền trong appsettings.Template.json hoặc inject qua CI/CD
/// (GitHub Secrets → biến môi trường). Chưa điền đủ thì toàn bộ tính năng GA4 tự tắt êm (không lỗi).
/// </summary>
public class GoogleAnalytics4Options
{
    public const string SectionName = "GoogleAnalytics4";

    /// <summary>Property ID dạng số của GA4 (VD: 123456789) — dùng cho Data API đọc chỉ số.</summary>
    public string PropertyId { get; set; } = string.Empty;

    /// <summary>Measurement ID của web stream Store (G-XXXXXXXXXX) — trả ra frontend qua endpoint public-config.</summary>
    public string MeasurementId { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung file JSON key Service Account — DÁN NGUYÊN OBJECT từ file key tải về (không cần escape).
    /// VD: "ServiceAccount": { "type": "service_account", "project_id": "...", "private_key": "...", "client_email": "..." }
    /// </summary>
    public Dictionary<string, string> ServiceAccount { get; set; } = new();

    /// <summary>API Secret của Measurement Protocol (để Mobile gửi sự kiện qua Backend proxy).</summary>
    public string MeasurementProtocolApiSecret { get; set; } = string.Empty;
}
