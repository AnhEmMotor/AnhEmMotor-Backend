using System;

namespace Domain.Constants
{
    public static class SupplierStatus
    {
        public const string Active = "active";
        public const string Inactive = "inactive";

        public static readonly string[] AllowedValues = [ Active, Inactive ];

        public static bool IsValid(string? value)
        {
            if(string.IsNullOrWhiteSpace(value))
                return false;

            return AllowedValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
