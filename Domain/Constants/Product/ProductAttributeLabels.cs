
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

    public static readonly string[] InternalProperties = [ "Id", "Name", "Description", "ShortDescription", "MetaTitle", "MetaDescription", "Brand", "ProductCategory", "ProductStatus", "ProductVariants", "BrandId", "CategoryId", "StatusId", "CreatedAt", "UpdatedAt", "DeletedAt", "CreatedBy", "UpdatedBy" ];

    public static bool IsInternalProperty(string propertyName)
    {
        foreach(var internalProperty in InternalProperties)
        {
            if(string.Compare(propertyName, internalProperty, StringComparison.Ordinal) is 0)
            {
                return true;
            }
        }

        return false;
    }
}
