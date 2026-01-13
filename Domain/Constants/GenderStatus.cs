namespace Domain.Constants;

public static class GenderStatus
{
    public const string Male = "Male";

    public const string Female = "Female";

    public const string Other = "Other";

    public static readonly List<string> All = [ Male, Female, Other ];

    public static bool IsValid(string? value) => !string.IsNullOrWhiteSpace(value) && All.Contains(value);
}
