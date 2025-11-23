using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Constants
{
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
}
