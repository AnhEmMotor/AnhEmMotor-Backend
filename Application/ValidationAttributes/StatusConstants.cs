namespace Application.ValidationAttributes
{
    public static class StatusConstants
    {
        public static class SupplierStatus
        {
            public const string Active = "active";
            public const string Inactive = "inactive";

            public static readonly string[] AllowedValues = [Active, Inactive];

            public static bool IsValid(string? value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;

                return AllowedValues.Contains(value, StringComparer.OrdinalIgnoreCase);
            }
        }

        public static class ProductStatus
        {
            public const string ForSale = "for-sale";
            public const string OutOfBusiness = "out-of-business";

            public static readonly string[] AllowedValues = [ForSale, OutOfBusiness];

            public static bool IsValid(string? value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;

                return AllowedValues.Contains(value, StringComparer.OrdinalIgnoreCase);
            }
        }

        public static class InputStatus
        {
            public const string Working = "working";
            public const string Finish = "finished";
            public const string Cacncel = "cancelled";

            public static readonly string[] AllowedValues = [Working, Finish, Cacncel];

            public static bool IsValid(string? value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;

                return AllowedValues.Contains(value, StringComparer.OrdinalIgnoreCase);
            }
        }

        public static class OutputStatus
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

            public static readonly string[] AllowedValues =
            [
                Completed,
                ConfirmedCod,
                Delivering,
                DepositPaid,
                PaidProcessing,
                Pending,
                Refunded,
                Refunding,
                WaitingDeposit,
                WaitingPickup,
                Cancelled
            ];


            public static bool IsValid(string? value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;

                return AllowedValues.Contains(value, StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
