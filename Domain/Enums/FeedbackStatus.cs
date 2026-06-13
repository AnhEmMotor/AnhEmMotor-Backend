namespace Domain.Enums;

public static class FeedbackStatus
{
    public const string Pending = "Pending";
    public const string Read = "Read";
    public const string Resolved = "Resolved";

    public static readonly string[] All = [Pending, Read, Resolved];
}
