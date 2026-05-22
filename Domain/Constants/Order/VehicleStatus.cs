namespace Domain.Constants.Order;

public static class VehicleStatus
{
    public const string Available = "Available";
    public const string AssignedToOrder = "AssignedToOrder";
    public const string Sold = "Sold";

    public static readonly HashSet<string> All = [Available, AssignedToOrder, Sold];

    public static bool IsValid(string? status)
    {
        return !string.IsNullOrWhiteSpace(status) &&
            All.Contains(status, StringComparer.OrdinalIgnoreCase);
    }
}
