namespace Domain.Constants.Order;

public static class OrderLockStatus
{
    public static readonly HashSet<string> LockedStatuses =[ OrderStatus.ConfirmedCod, OrderStatus.PaidProcessing, OrderStatus.DepositPaid, OrderStatus.Delivering, OrderStatus.WaitingPickup, OrderStatus.Completed, OrderStatus.Cancelled, OrderStatus.Refunding, OrderStatus.Refunded ];
}
