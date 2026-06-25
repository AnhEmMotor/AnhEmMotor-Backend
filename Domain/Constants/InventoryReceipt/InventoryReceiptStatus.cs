using System;

namespace Domain.Constants.InventoryReceipt
{
    public static class InventoryReceiptStatus
    {
        public const string Draft = "draft";
        public const string Sent = "sent";
        public const string Approve = "approve";
        public const string Reject = "reject";

        public static readonly string[] AllowedValues = [Draft, Sent, Approve, Reject];
        public static readonly string[] FinishInventoryReceiptValues = [Approve];
        public static readonly string[] WorkingInventoryReceiptValues = [Draft, Sent];
        public static readonly string[] NotEditInventoryReceiptValues = [Approve, Reject];

        public static bool IsValid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return AllowedValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsFinished(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return FinishInventoryReceiptValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsCanEdit(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return WorkingInventoryReceiptValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsCannotEdit(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            return NotEditInventoryReceiptValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsCannotDelete(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            return string.Equals(value, Approve, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, Reject, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsDraft(string? value)
        {
            return string.Equals(value, Draft, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsSent(string? value)
        {
            return string.Equals(value, Sent, StringComparison.OrdinalIgnoreCase);
        }
    }
}
