using Application.Common.Interfaces;

namespace IntegrationTests.SetupClass;

/// <summary>
/// "Bây giờ" cố định cho test biên giờ VN (Stage 16.6) — không dùng đồng hồ hệ thống.
/// </summary>
public class FakeServerDateProvider(DateTimeOffset fixedUtcNow) : IServerDateProvider
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

    public DateTimeOffset UtcNow => fixedUtcNow;

    public DateTimeOffset VietnamNow => fixedUtcNow.ToOffset(VietnamOffset);

    public DateOnly VietnamToday => DateOnly.FromDateTime(VietnamNow.Date);

    public (DateTime StartUtc, DateTime EndUtc) VietnamDayRangeUtc(DateOnly vietnamDate)
    {
        var startVietnam = new DateTimeOffset(vietnamDate.ToDateTime(TimeOnly.MinValue), VietnamOffset);
        var endVietnam = startVietnam.AddDays(1);
        return (startVietnam.UtcDateTime, endVietnam.UtcDateTime);
    }
}
