using System;

namespace Domain.Constants.InventoryReceipt
{
    public static class InputStatus
    {
        public const string Draft = "draft";
        public const string Sent = "sent";
        public const string Approve = "approve";
        public const string Reject = "reject";

        // Maintain legacy values to prevent mapping errors if they are referenced elsewhere, but transition to new ones
        public const string Working = "draft";
        public const string Finish = "approve";
        public const string Cancel = "reject";

        public static readonly string[] AllowedValues = [Draft, Sent, Approve, Reject, "working", "finished", "cancelled"];
        public static readonly string[] FinishInputValues = [Approve, "finished"];
        public static readonly string[] WorkingInputValues = [Draft, Sent, "working"];
        public static readonly string[] NotEditInputValues = [Approve, Reject, "finished", "cancelled"];

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
            return FinishInputValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsCanEdit(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return WorkingInputValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsCannotEdit(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            return NotEditInputValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsCannotDelete(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            return string.Equals(value, Approve, StringComparison.OrdinalIgnoreCase) || 
                   string.Equals(value, "finished", StringComparison.OrdinalIgnoreCase);
        }
    }
}
