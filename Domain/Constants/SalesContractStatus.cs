namespace Domain.Constants;

public static class SalesContractStatus
{
    public const string Draft = "Draft";
    public const string Signed = "Signed";
    public const string Fulfilled = "Fulfilled";

    public static readonly string[] AllowedValues = [Draft, Signed, Fulfilled];

    public static readonly List<string> All = [Draft, Signed, Fulfilled];

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return AllowedValues.Contains(value, StringComparer.OrdinalIgnoreCase);
    }
}
