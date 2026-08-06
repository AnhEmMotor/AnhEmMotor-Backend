using FluentAssertions;
using Infrastructure.Services;

namespace UnitTests;

public class ServerDateProvider
{
    private readonly SystemServerDateProvider _provider = new();

    [Fact(DisplayName = "SERVERDATE_01 - Unit - VietnamDayRangeUtc trả đúng biên UTC cho 1 ngày giờ VN")]
    public void VietnamDayRangeUtc_ReturnsCorrectUtcBoundaries()
    {
        var (start, end) = _provider.VietnamDayRangeUtc(new DateOnly(2026, 7, 26));
        start.Should().Be(new DateTime(2026, 7, 25, 17, 0, 0, DateTimeKind.Utc));
        end.Should().Be(new DateTime(2026, 7, 26, 17, 0, 0, DateTimeKind.Utc));
    }

    [Fact(DisplayName = "SERVERDATE_02 - Unit - VietnamNow cộng đúng 7 giờ so với UtcNow")]
    public void VietnamNow_IsSevenHoursAheadOfUtc()
    {
        var utc = _provider.UtcNow;
        var vietnam = _provider.VietnamNow;
        (vietnam.UtcDateTime - utc.UtcDateTime).Should().BeCloseTo(TimeSpan.Zero, TimeSpan.FromSeconds(1));
        vietnam.Offset.Should().Be(TimeSpan.FromHours(7));
    }

    [Fact(DisplayName = "SERVERDATE_03 - Unit - VietnamToday không lệch ngày trong khung 00:00-07:00 giờ VN")]
    public void VietnamToday_DoesNotLagBehindDuringEarlyMorningVietnamHours()
    {
        var (_, endOfJuly25Utc) = _provider.VietnamDayRangeUtc(new DateOnly(2026, 7, 25));
        var vietnamNowAtBoundary = new DateTimeOffset(endOfJuly25Utc, TimeSpan.Zero).ToOffset(TimeSpan.FromHours(7));
        DateOnly.FromDateTime(vietnamNowAtBoundary.Date).Should().Be(new DateOnly(2026, 7, 26));
    }
}
