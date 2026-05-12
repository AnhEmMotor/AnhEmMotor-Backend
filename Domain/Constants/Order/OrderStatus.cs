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
            "pending" => "Chờ xác nhận",
            "confirmedcod" => "Đã xác nhận (Chờ thanh toán COD)",
            "paidprocessing" => "Đã thanh toán (Chờ xử lý)",
            "waitingdeposit" => "Chờ đặt cọc",
            "depositpaid" => "Đã đặt cọc (Chờ xử lý)",
            "waitinginstallment" => "Chờ duyệt trả góp",
            "installmentapproved" => "Đã duyệt trả góp (Chờ xử lý)",
            "delivering" => "Đang giao hàng",
            "waitingpickup" => "Chờ lấy hàng tại cửa hàng",
            "completed" => "Đã hoàn thành",
            "cancelled" => "Đã hủy",
            "refunding" => "Đang hoàn tiền",
            "refunded" => "Đã hoàn tiền",
            _ => status
        };
    }
}
