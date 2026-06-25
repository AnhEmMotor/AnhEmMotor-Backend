using System;
using System.Linq;

namespace Domain.Constants
{
    public static class PartnerType
    {
        public const string Supplier = "supplier";
        public const string Financial = "financial";
        public const string Insurance = "insurance";

        public static readonly IReadOnlyDictionary<string, string> TypeNames = new Dictionary<string, string>
        { { Supplier, "Nhà cung cấp" }, { Financial, "Tài chính" }, { Insurance, "Bảo hiểm" } };

        public static readonly string[] AllowedValues = [Supplier, Financial, Insurance];

        public static readonly List<string> All = [Supplier, Financial, Insurance];

        public static string GetName(string? key) => key != null && TypeNames.TryGetValue(key, out var name)
            ? name
            : "Không xác định";

        public static string GetKeyFromName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;
            var match = TypeNames.FirstOrDefault(x => x.Value.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
            return match.Key ?? string.Empty;
        }

        public static string ExcelValidationList => $"\"{string.Join(",", TypeNames.Values)}\"";

        public static bool IsValid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return AllowedValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
