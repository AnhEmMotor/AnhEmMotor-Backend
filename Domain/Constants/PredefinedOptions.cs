namespace Domain.Constants
{
    public static class PredefinedOptions
    {
        public static readonly IReadOnlyDictionary<string, string> Options = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase)
        {
            { "VehicleType", "Loại xe" },
            { "Displacement", "Phân khối" },
            { "Condition", "Tình trạng" },
            { "Version", "Phiên bản" },
            { "ManufactureYear", "Năm sản xuất" },
            { "BrakeSystem", "Hệ thống phanh" },
            { "Size", "Kích cỡ" },
            { "Material", "Chất liệu" },
            { "Style", "Phong cách" }
        };
    }
}
