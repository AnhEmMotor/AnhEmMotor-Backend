using Application.ApiContracts.Product.Responses;

namespace Application.ApiContracts.Client.Catalog
{
    public class ProductSummaryResponse
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public int? CategoryId { get; set; }

        public string? Category { get; set; }

        public int? BrandId { get; set; }

        public string? Brand { get; set; }

        public decimal? Displacement { get; set; }

        public string? ShortDescription { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public string? MetaTitle { get; set; }

        public string? MetaDescription { get; set; }

        public string? Origin { get; set; }

        public string? Unit { get; set; }

        public decimal? Weight { get; set; }

        public decimal? Length { get; set; }

        public decimal? Width { get; set; }

        public decimal? Height { get; set; }

        public string? Wheelbase { get; set; }

        public decimal? SeatHeight { get; set; }

        public string? GroundClearance { get; set; }

        public string? FuelCapacity { get; set; }

        public string? TireSize { get; set; }

        public string? FrontSuspension { get; set; }

        public string? RearSuspension { get; set; }

        public string? EngineType { get; set; }

        public string? MaxPower { get; set; }

        public string? TransmissionType { get; set; }

        public string? StarterSystem { get; set; }

        public string? MaxTorque { get; set; }

        public string? BoreStroke { get; set; }

        public string? CompressionRatio { get; set; }

        public string? FuelSystem { get; set; }

        public string? FrameType { get; set; }

        public string? FrontTireSize { get; set; }

        public string? RearTireSize { get; set; }

        public string? FrontBrake { get; set; }

        public string? RearBrake { get; set; }

        public string? BatteryType { get; set; }

        public string? LightingSystem { get; set; }

        public string? DashboardType { get; set; }

        public string? Material { get; set; }

        public string? WarrantyPeriod { get; set; }

        public decimal? OilCapacity { get; set; }

        public string? FuelConsumption { get; set; }

        public int? ProductLimit { get; set; }

        public int? EffectiveMax { get; set; }

        public decimal ReferencePrice { get; set; }

        public string? PromotionText { get; set; }

        public List<ProductTechnologyResponse> Technologies { get; set; } = new();

        public List<ProductVariantSummaryResponse> Variants { get; set; } = new();
    }

    public class ProductVariantSummaryResponse
    {
        public int Id { get; set; }

        public string? UrlSlug { get; set; }

        public string? VariantName { get; set; }

        public string? OptionValuesText { get; set; }

        public string? DisplayName { get; set; }

        public decimal? Price { get; set; }

        public string? CoverImageUrl { get; set; }

        public List<string> Photos { get; set; } = new();

        public List<ProductColorSummaryResponse> Colors { get; set; } = new();

        public int? ProductLimit { get; set; }

        public int? EffectiveMax { get; set; }
    }

    public class ProductColorSummaryResponse
    {
        public int Id { get; set; }

        public string? ColorName { get; set; }

        public string? ColorCode { get; set; }

        public string? CoverImageUrl { get; set; }

        public int? MaxPurchaseQuantity { get; set; }
    }

    public class ProductDetailResponse
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal ReferencePrice { get; set; }

        public List<string> Features { get; set; } = new();

        public bool IsCompatibleWithMyVehicle { get; set; }

        public string? CompatibilityNote { get; set; }
    }

    public class ConsultationRequest
    {
        public int ProductId { get; set; }

        public string? CustomerNote { get; set; }

        public string? PreferredContactTime { get; set; }
    }
}
