namespace Domain.Enums;

public static class SupportRequestStatus
{
public const string New = "New";
public const string Assigned = "Assigned";
public const string InProgress = "InProgress";
public const string Closed = "Closed";

public static readonly string[] All = [New, Assigned, InProgress, Closed];
}
