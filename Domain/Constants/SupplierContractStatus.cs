namespace Domain.Constants;

public static class SupplierContractStatus
{
    public const string Draft = "Draft";
    public const string PendingApproval = "PendingApproval";
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string Terminated = "Terminated";
    public const string Completed = "Completed";

    public static readonly string[] AllowedValues = [Draft, PendingApproval, Active, Expired, Terminated, Completed];

    public static readonly List<string> All = [Draft, PendingApproval, Active, Expired, Terminated, Completed];

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        return AllowedValues.Contains(value, StringComparer.OrdinalIgnoreCase);
    }
}