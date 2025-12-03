namespace Domain.Constants;

public static class OrderStatusTransitions
{
    private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new()
    {
        {
            OrderStatus.Pending,
            [ OrderStatus.ConfirmedCod, OrderStatus.PaidProcessing, OrderStatus.WaitingDeposit, OrderStatus.Cancelled ]
        },
        {
            OrderStatus.ConfirmedCod,
            [ OrderStatus.Delivering, OrderStatus.WaitingPickup, OrderStatus.Completed, OrderStatus.Cancelled ]
        },
        {
            OrderStatus.PaidProcessing,
            [ OrderStatus.Delivering, OrderStatus.WaitingPickup, OrderStatus.Completed, OrderStatus.Refunding ]
        },
        { OrderStatus.WaitingDeposit, [ OrderStatus.DepositPaid, OrderStatus.Cancelled ] },
        {
            OrderStatus.DepositPaid,
            [ OrderStatus.Delivering, OrderStatus.WaitingPickup, OrderStatus.Completed, OrderStatus.Refunding ]
        },
        { OrderStatus.Delivering, [ OrderStatus.Completed, OrderStatus.Refunding ] },
        { OrderStatus.WaitingPickup, [ OrderStatus.Completed, OrderStatus.Refunding ] },
        { OrderStatus.Cancelled, [ OrderStatus.Pending ] },
        { OrderStatus.Refunding, [ OrderStatus.Refunded, OrderStatus.Pending ] },
        { OrderStatus.Refunded, [ OrderStatus.Pending ] },
        { OrderStatus.Completed, [] }
    };

    public static bool IsTransitionAllowed(string? currentStatus, string? newStatus)
    {
        if(string.IsNullOrWhiteSpace(currentStatus) || string.IsNullOrWhiteSpace(newStatus))
        {
            return false;
        }

        if(!AllowedTransitions.TryGetValue(currentStatus, out var allowedStatuses))
        {
            return false;
        }

        return allowedStatuses.Contains(newStatus);
    }

    public static HashSet<string> GetAllowedTransitions(string? currentStatus)
    {
        if(string.IsNullOrWhiteSpace(currentStatus))
        {
            return [];
        }

        return AllowedTransitions.TryGetValue(currentStatus, out var allowed) ? allowed : [];
    }
}
