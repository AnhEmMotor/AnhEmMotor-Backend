namespace Application.Features.ChatTools.Common;

public static class ChatToolDateRange
{
    private const int DefaultDays = 30;
    private const int MaxSpanDays = 365;

    /// <summary>Suy ra khoảng (start, end) dạng UTC từ FromDate/ToDate tuỳ chọn của tool — thiếu thì lấy 30 ngày gần nhất, khoảng quá dài thì cắt về tối đa 365 ngày.</summary>
    public static (DateTimeOffset Start, DateTimeOffset End) Resolve(DateOnly? fromDate, DateOnly? toDate)
    {
        var end = toDate.HasValue
            ? toDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc)
            : DateTimeOffset.UtcNow;
        var start = fromDate.HasValue
            ? fromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
            : end.AddDays(-DefaultDays);

        if ((end - start).TotalDays > MaxSpanDays)
        {
            start = end.AddDays(-MaxSpanDays);
        }

        return (start, end);
    }
}
