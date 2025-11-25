namespace Domain.Constants
{
    public static class OrderStatus
    {
        public const string Completed = "completed";
        public const string ConfirmedCod = "confirmed_cod";
        public const string Delivering = "delivering";
        public const string DepositPaid = "deposit_paid";
        public const string PaidProcessing = "paid_processing";
        public const string Pending = "pending";
        public const string Refunded = "refunded";
        public const string Refunding = "refunding";
        public const string WaitingDeposit = "waiting_deposit";
        public const string WaitingPickup = "waiting_pickup";
        public const string Cancelled = "cancelled";

        public static readonly HashSet<string> All =[ Completed, ConfirmedCod, Delivering, DepositPaid, PaidProcessing, Pending, Refunded, Refunding, WaitingDeposit, WaitingPickup, Cancelled ];

        public static readonly HashSet<string> BookingPhases =[ ConfirmedCod, PaidProcessing, WaitingDeposit, DepositPaid, Delivering, WaitingPickup ];

        public static bool IsValid(string? value)
        {
            if(string.IsNullOrWhiteSpace(value))
                return false;
            return All.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsBookingStatus(string? value)
        {
            if(string.IsNullOrWhiteSpace(value))
                return false;
            return BookingPhases.Contains(value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
