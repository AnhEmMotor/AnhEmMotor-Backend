using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class SystemServerDateProvider : IServerDateProvider
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateTimeOffset VietnamNow => UtcNow.ToOffset(VietnamOffset);

    public DateOnly VietnamToday => DateOnly.FromDateTime(VietnamNow.Date);

    public (DateTime StartUtc, DateTime EndUtc) VietnamDayRangeUtc(DateOnly vietnamDate)
    {
        var startVietnam = new DateTimeOffset(vietnamDate.ToDateTime(TimeOnly.MinValue), VietnamOffset);
        var endVietnam = startVietnam.AddDays(1);
        return (startVietnam.UtcDateTime, endVietnam.UtcDateTime);
    }
}
