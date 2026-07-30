namespace Application.Common.Interfaces;

public interface IServerDateProvider
{
    DateTimeOffset UtcNow { get; }

    /// <summary>Giờ hiện tại theo GMT+7 (Asia/Ho_Chi_Minh) — offset cố định, không dùng TimeZoneInfo hệ điều hành.</summary>
    DateTimeOffset VietnamNow { get; }

    DateOnly VietnamToday { get; }

    /// <summary>Khoảng UTC [Start, End) tương ứng đúng 1 ngày theo giờ Việt Nam — dùng để query DB (lưu UTC).</summary>
    (DateTime StartUtc, DateTime EndUtc) VietnamDayRangeUtc(DateOnly vietnamDate);
}
