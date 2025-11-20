namespace Domain.Constants;

public static class OrderBookingStatuses
{
    public const string ConfirmedCod = "confirmed_cod";
    public const string PaidProcessing = "paid_processing";
    public const string WaitingDeposit = "waiting_deposit";
    public const string DepositPaid = "deposit_paid";
    public const string Delivering = "delivering";
    public const string WaitingPickup = "waiting_pickup";

    public static readonly string[] All =
    [
        ConfirmedCod,
        PaidProcessing,
        WaitingDeposit,
        DepositPaid,
        Delivering,
        WaitingPickup
    ];

    public static bool IsBookingStatus(string? statusId)
    {
        return !string.IsNullOrWhiteSpace(statusId) && All.Contains(statusId);
    }
}
