using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Constants.Product
{
    public static class ProductManagementType
    {
        public const string Sku = "sku";
        public const string VinNumber = "vin_number";

        public static readonly HashSet<string> All = [Sku, VinNumber];

        public static bool IsValid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return All.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static string GetDisplayName(string type) => type.ToLower() switch
        {
            "sku" => "Quản lý theo SKU",
            "vin_number" => "Quản lý theo số VIN",
            _ => type
        };

        public static IEnumerable<object> GetActiveList()
        {
            return All.Select(type => new
            {
                Value = type,
                Text = GetDisplayName(type)
            });
        }
    }
}