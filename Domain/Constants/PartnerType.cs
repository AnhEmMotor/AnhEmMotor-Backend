using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Constants
{
    public static class PartnerType
    {
        public const string Supplier = "supplier";
        public const string Financial = "financial";
        public const string Insurance = "insurance";

        public static readonly string[] AllowedValues = [Supplier, Financial, Insurance];

        public static readonly List<string> All = [Supplier, Financial, Insurance];

        public static string GetName(string? key) => key switch
        {
            Supplier => "Nhà cung cấp hàng hóa",
            Financial => "Đối tác tài chính",
            Insurance => "Đối tác bảo hiểm",
            _ => "Không xác định"
        };

        public static bool IsValid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return AllowedValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
