
namespace Domain.Constants.Product;

public static class ProductAttributeLabels
{
    public static readonly Dictionary<string, string> Labels = new()
    {
        { "Weight", "Trọng lượng" },
        { "Dimensions", "Kích thước (Dài x Rộng x Cao)" },
        { "Wheelbase", "Khoảng cách trục bánh xe" },
        { "SeatHeight", "Độ cao yên" },
        { "GroundClearance", "Khoảng sáng gầm xe" },
        { "FuelCapacity", "Dung tích bình xăng" },
        { "TireSize", "Kích cỡ lốp" },
        { "FrontSuspension", "Phuộc trước" },
        { "RearSuspension", "Phuộc sau" },
        { "EngineType", "Loại động cơ" },
        { "MaxPower", "Công suất tối đa" },
        { "OilCapacity", "Dung tích nhớt" },
        { "FuelConsumption", "Mức tiêu thụ nhiên liệu" },
        { "TransmissionType", "Loại truyền động" },
        { "StarterSystem", "Hệ thống khởi động" },
        { "MaxTorque", "Mô-men xoắn cực đại" },
        { "Displacement", "Dung tích xy-lanh" },
        { "BoreStroke", "Đường kính x Hành trình pít-tông" },
        { "CompressionRatio", "Tỉ số nén" }
    };

    private static readonly HashSet<string> TechnicalSpecExclusions = new(StringComparer.Ordinal)
    {
        "Id", "Name", "Description", "ShortDescription", "MetaTitle", 
        "MetaDescription", "Brand", "ProductCategory", "ProductStatus", 
        "ProductVariants", "ProductTechnologies", "Highlights", 
        "BrandId", "CategoryId", "StatusId", "CreatedAt", "UpdatedAt", 
        "DeletedAt", "CreatedBy", "UpdatedBy"
    };

    public static bool IsInternalProperty(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) return false;
        return TechnicalSpecExclusions.Contains(propertyName);
    }
}
