using Application.Common.Models;
using Application.Common.Models.Ga4;

namespace Application.Interfaces.Services.Analytics;

/// <summary>
/// Đọc chỉ số Google Analytics 4 qua Data API (dùng Service Account — credential CHỈ nằm ở Backend).
/// </summary>
public interface IGa4AnalyticsService
{
    /// <summary>GA4 đã được cấu hình đủ (Enabled + PropertyId + key) hay chưa.</summary>
    public bool IsConfigured();

    /// <summary>
    /// Chạy report tổng quát: tổng cả kỳ khi không truyền dimension, hoặc phân rã theo một chiều
    /// ("date", "sessionSource", "pagePath", "deviceCategory"...).
    /// </summary>
    public Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> RunReportAsync(Ga4ReportRequest request, CancellationToken cancellationToken);

    /// <summary>Tổng quan cả kỳ: sessions, users, pageviews, engagement...</summary>
    public Task<Result<Ga4OverviewDto>> GetOverviewAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);

    /// <summary>Chuỗi số liệu theo ngày phục vụ vẽ biểu đồ.</summary>
    public Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> GetDailySeriesAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);

    /// <summary>Top nguồn traffic (sessionSource / sessionMedium).</summary>
    public Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> GetTopSourcesAsync(DateOnly startDate, DateOnly endDate, int limit, CancellationToken cancellationToken);

    /// <summary>Top trang được xem nhiều nhất.</summary>
    public Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> GetTopPagesAsync(DateOnly startDate, DateOnly endDate, int limit, CancellationToken cancellationToken);

    /// <summary>Phân rã theo loại thiết bị.</summary>
    public Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> GetDeviceBreakdownAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);

    /// <summary>Top trang theo TIÊU ĐỀ trang (pageTitle) theo lượt xem.</summary>
    public Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> GetTopPageTitlesAsync(DateOnly startDate, DateOnly endDate, int limit, CancellationToken cancellationToken);

    /// <summary>Phân rã người dùng theo hệ điều hành (operatingSystem).</summary>
    public Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> GetOperatingSystemBreakdownAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);

    /// <summary>Phân rã người dùng theo trình duyệt (browser).</summary>
    public Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> GetBrowserBreakdownAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);

    /// <summary>Chỉ số realtime 30 phút qua: người dùng hoạt động, lượt xem, nguồn traffic, thiết bị.</summary>
    public Task<Result<Ga4RealtimeDto>> GetRealtimeAsync(CancellationToken cancellationToken);
}
