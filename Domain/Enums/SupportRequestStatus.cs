namespace Domain.Enums;

public static class SupportRequestStatus
{
    public const string New = "New";
    public const string InProgress = "InProgress";
    public const string Closed = "Closed";

    public static readonly string[] All = [New, InProgress, Closed];
}
