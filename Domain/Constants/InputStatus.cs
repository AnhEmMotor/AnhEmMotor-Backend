using System;

namespace Domain.Constants
{
    public static class InputStatus
    {
        public const string Working = "working";
        public const string Finish = "finished";
        public const string Cancel = "cancelled";

        public static readonly string[] AllowedValues = [ Working, Finish, Cancel];

        public static bool IsValid(string? value)
        {
            if(string.IsNullOrWhiteSpace(value))
                return false;

            return AllowedValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
