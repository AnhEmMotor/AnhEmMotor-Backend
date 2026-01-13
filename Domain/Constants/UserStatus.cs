namespace Domain.Constants;

public static class UserStatus
{
    public const string Active = "Active";

    public const string Banned = "Banned";

    public static readonly List<string> All = [ Active, Banned ];
    public static bool IsValid(string? value) => !string.IsNullOrWhiteSpace(value) && All.Contains(value);
}
