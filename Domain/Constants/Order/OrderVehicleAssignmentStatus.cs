
namespace Domain.Constants.Order;

public static class OrderVehicleAssignmentStatus
{
    public static readonly HashSet<string> RequiredStatuses = [OrderStatus.Delivering, OrderStatus.WaitingPickup, OrderStatus.Completed];

    public static bool RequiresVehicleAssignment(string? statusId)
    {
        return !string.IsNullOrWhiteSpace(statusId) &&
            RequiredStatuses.Contains(statusId, StringComparer.OrdinalIgnoreCase);
    }

    public static HashSet<string> ReturnVehicleAssignmentStatus()
    {
        return RequiredStatuses;
    }
}
