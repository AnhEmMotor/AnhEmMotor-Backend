using Application.Common.Interfaces;

namespace Application.Features.ChatTools.Common;

public static class ChatToolDateRange
{
    private const int DefaultDays = 30;
    private const int MaxSpanDays = 365;

    /// <summary>
    /// Suy ra khoảng (start, end) dạng UTC từ FromDate/ToDate tuỳ chọn của tool — thiếu thì lấy 30 ngày gần nhất tính
    /// đến HÔM NAY THEO GIỜ VIỆT NAM (Stage 16.2 mục #2 — không được suy "hôm nay" theo UTC trần vì lệch ngày trong
    /// khung 00:00–07:00 giờ VN), khoảng quá dài thì cắt về tối đa 365 ngày.
    /// </summary>
    public static (DateTimeOffset Start, DateTimeOffset End) Resolve(
        DateOnly? fromDate,
        DateOnly? toDate,
        IServerDateProvider dateProvider)
    {
        var today = dateProvider.VietnamToday;
        var end = toDate.HasValue
            ? AsUtcOffset(dateProvider.VietnamDayRangeUtc(toDate.Value).EndUtc).AddTicks(-1)
            : AsUtcOffset(dateProvider.VietnamDayRangeUtc(today).EndUtc).AddTicks(-1);
        var start = fromDate.HasValue
            ? AsUtcOffset(dateProvider.VietnamDayRangeUtc(fromDate.Value).StartUtc)
            : end.AddDays(-DefaultDays);
        if ((end - start).TotalDays > MaxSpanDays)
        {
            start = end.AddDays(-MaxSpanDays);
        }
        return (start, end);
    }

    /// <summary>
    /// Chuỗi "yyyy-MM-dd đến yyyy-MM-dd" theo NGÀY GIỜ VN của khoảng (start, end) — để lộ ra envelope's FiltersApplied
    /// cho AI/người dùng thấy tool đã tính "hôm nay"/"tháng này" ra đúng ngày nào.
    /// </summary>
    public static string FormatVietnamRange(DateTimeOffset start, DateTimeOffset end)
    {
        var vnStart = start.ToOffset(TimeSpan.FromHours(7)).Date;
        var vnEnd = end.ToOffset(TimeSpan.FromHours(7)).Date;
        return $"{vnStart:yyyy-MM-dd} đến {vnEnd:yyyy-MM-dd}";
    }

    private static DateTimeOffset AsUtcOffset(DateTime utcDateTime) => new(
        DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc));
}
