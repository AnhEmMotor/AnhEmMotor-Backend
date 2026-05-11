namespace Domain.Constants.Order
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
        public const string WaitingInstallment = "waiting_installment";
        public const string InstallmentApproved = "installment_approved";

        public static readonly HashSet<string> All = [Completed, ConfirmedCod, Delivering, DepositPaid, PaidProcessing, Pending, Refunded, Refunding, WaitingDeposit, WaitingPickup, Cancelled, WaitingInstallment, InstallmentApproved];

        public static readonly HashSet<string> BookingPhases = [ConfirmedCod, Delivering, DepositPaid, PaidProcessing, Pending, Refunded, Refunding, WaitingDeposit, WaitingPickup, WaitingInstallment, InstallmentApproved];

        public static readonly HashSet<string> NotDeletedPhases = [Completed, Refunded, Cancelled];

        public static bool IsValid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return All.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsBookingStatus(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return BookingPhases.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsCannotDelete(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return NotDeletedPhases.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static string GetDisplayName(string status) => status.ToLower() switch
        {
            Pending => "Ch? xác nh?n",
            ConfirmedCod => "Ðã xác nh?n (Ch? thanh toán COD)",
            PaidProcessing => "Ðã thanh toán (Ch? x? lý)",
            WaitingDeposit => "Ch? d?t c?c",
            DepositPaid => "Ðã d?t c?c (Ch? x? lý)",
            WaitingInstallment => "Ch? duy?t tr? góp",
            InstallmentApproved => "Ðã duy?t tr? góp (Ch? x? lý)",
            Delivering => "Ðang giao hàng",
            WaitingPickup => "Ch? l?y hàng t?i c?a hàng",
            Completed => "Ðã hoàn thành",
            Cancelled => "Ðã h?y",
            Refunding => "Ðang hoàn ti?n",
            Refunded => "Ðã hoàn ti?n",
            _ => status
        };
    }
}
