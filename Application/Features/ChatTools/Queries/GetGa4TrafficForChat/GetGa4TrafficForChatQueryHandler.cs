using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Ga4;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Services.Analytics;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetGa4TrafficForChat;

/// <summary>
/// Đọc chỉ số Google Analytics 4 (traffic web Store + app Mobile) qua Data API — số liệu người dùng thật,
/// không phải dữ liệu bán hàng nội bộ. Nguồn số liệu là GA4 nên có thể lệch nhẹ so với hệ thống đơn hàng.
/// </summary>
public class GetGa4TrafficForChatQueryHandler(
    IGa4AnalyticsService ga4AnalyticsService,
    IServerDateProvider dateProvider) : IRequestHandler<GetGa4TrafficForChatQuery, Result<ChatToolEnvelope<Ga4TrafficRowDto>>>
{
    public async Task<Result<ChatToolEnvelope<Ga4TrafficRowDto>>> Handle(
        GetGa4TrafficForChatQuery request,
        CancellationToken cancellationToken)
    {
        if (!ga4AnalyticsService.IsConfigured())
        {
            return Result<ChatToolEnvelope<Ga4TrafficRowDto>>.Failure(
                "Google Analytics 4 chưa được cấu hình trên server (thiếu PropertyId hoặc Service Account key).");
        }

        var today = dateProvider.VietnamToday;
        var end = request.ToDate.HasValue && request.ToDate.Value <= today ? request.ToDate.Value : today;
        var start = request.FromDate.HasValue && request.FromDate.Value < end ? request.FromDate.Value : end.AddDays(-29);

        var (dimension, effectiveLimit) = ResolveBreakdown(request.Breakdown, request.Limit);
        var report = await ga4AnalyticsService
            .RunReportAsync(new Ga4ReportRequest { StartDate = start, EndDate = end, Dimension = dimension, Limit = effectiveLimit }, cancellationToken)
            .ConfigureAwait(false);
        if (report.IsFailure)
        {
            return Result<ChatToolEnvelope<Ga4TrafficRowDto>>.Failure(report.Error!);
        }

        var items = report.Value.Rows.Select(MapRow).ToList();
        var inner = new ChatToolResult<Ga4TrafficRowDto>(items, items.Count, false);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "Google Analytics Data API (runReport)",
            new Dictionary<string, string>
            {
                ["Khoảng thời gian"] = $"{start:yyyy-MM-dd} đến {end:yyyy-MM-dd}",
                ["Phân rã"] = dimension is null
                    ? "Tổng cả kỳ"
                    : dimension switch
                    {
                        "date" => "Theo ngày",
                        "sessionSource" => "Theo nguồn traffic",
                        "pagePath" => "Theo trang",
                        _ => "Theo thiết bị"
                    }
            },
            "ga4-traffic",
            null);
        return ChatToolEnvelope<Ga4TrafficRowDto>.Wrap(inner, meta);
    }

    private static (string? Dimension, int Limit) ResolveBreakdown(string breakdown, int requestedLimit)
    {
        return (breakdown?.Trim().ToLowerInvariant()) switch
        {
            "day" or "date" or "ngay" => ("date", Math.Clamp(requestedLimit <= 0 ? 30 : requestedLimit, 1, 90)),
            "source" or "nguon" => ("sessionSource", ChatToolLimit.Clamp(requestedLimit)),
            "page" or "trang" => ("pagePath", ChatToolLimit.Clamp(requestedLimit)),
            "device" or "thiet-bi" => ("deviceCategory", 10),
            _ => (null, 1)
        };
    }

    private static Ga4TrafficRowDto MapRow(Ga4DimensionRowDto row) => new()
    {
        Label = row.Label,
        Sessions = row.Sessions,
        TotalUsers = row.TotalUsers,
        NewUsers = row.NewUsers,
        PageViews = row.ScreenPageViews,
        EngagementRate = row.EngagementRate,
        AvgSessionDurationSeconds = row.AverageSessionDuration,
        KeyEvents = row.KeyEvents
    };
}
