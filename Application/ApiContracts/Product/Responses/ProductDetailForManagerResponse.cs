using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Responses;

public class ProductDetailForManagerResponse
{
    public int? Id { get; set; }

    public string? Name { get; set; }

    [JsonPropertyName("category_id")]
    public int? CategoryId { get; set; }

    [JsonPropertyName("category")]
    public string? CategoryName { get; set; }

    [JsonPropertyName("brand_id")]
    public int? BrandId { get; set; }

    [JsonPropertyName("brand")]
    public string? BrandName { get; set; }

    public string? Description { get; set; }

    public decimal? Weight { get; set; }

    public string? Dimensions { get; set; }

    public decimal? Wheelbase { get; set; }

    [JsonPropertyName("seat_height")]
    public decimal? SeatHeight { get; set; }

    [JsonPropertyName("ground_clearance")]
    public decimal? GroundClearance { get; set; }

    [JsonPropertyName("fuel_capacity")]
    public decimal? FuelCapacity { get; set; }

    [JsonPropertyName("tire_size")]
    public string? TireSize { get; set; }

    [JsonPropertyName("front_suspension")]
    public string? FrontSuspension { get; set; }

    [JsonPropertyName("rear_suspension")]
    public string? RearSuspension { get; set; }

    [JsonPropertyName("engine_type")]
    public string? EngineType { get; set; }

    [JsonPropertyName("max_power")]
    public string? MaxPower { get; set; }

    [JsonPropertyName("oil_capacity")]
    public decimal? OilCapacity { get; set; }

    [JsonPropertyName("fuel_consumption")]
    public string? FuelConsumption { get; set; }

    [JsonPropertyName("transmission_type")]
    public string? TransmissionType { get; set; }

    [JsonPropertyName("starter_system")]
    public string? StarterSystem { get; set; }

    [JsonPropertyName("max_torque")]
    public string? MaxTorque { get; set; }

    public decimal? Displacement { get; set; }

    [JsonPropertyName("bore_stroke")]
    public string? BoreStroke { get; set; }

    [JsonPropertyName("compression_ratio")]
    public string? CompressionRatio { get; set; }

    [JsonPropertyName("fuel_system")]
    public string? FuelSystem { get; set; }

    [JsonPropertyName("frame_type")]
    public string? FrameType { get; set; }

    [JsonPropertyName("front_tire_size")]
    public string? FrontTireSize { get; set; }

    [JsonPropertyName("rear_tire_size")]
    public string? RearTireSize { get; set; }

    [JsonPropertyName("front_brake")]
    public string? FrontBrake { get; set; }

    [JsonPropertyName("rear_brake")]
    public string? RearBrake { get; set; }

    [JsonPropertyName("battery_type")]
    public string? BatteryType { get; set; }

    [JsonPropertyName("lighting_system")]
    public string? LightingSystem { get; set; }

    [JsonPropertyName("dashboard_type")]
    public string? DashboardType { get; set; }

    public string? Material { get; set; }

    public string? Origin { get; set; }

    [JsonPropertyName("warranty_period")]
    public string? WarrantyPeriod { get; set; }

    public string? Unit { get; set; }

    [JsonPropertyName("std_dot")]
    public bool StdDot { get; set; }

    [JsonPropertyName("std_ece")]
    public bool StdEce { get; set; }

    [JsonPropertyName("std_snell")]
    public bool StdSnell { get; set; }

    [JsonPropertyName("std_jis")]
    public bool StdJis { get; set; }

    [JsonPropertyName("other_standards")]
    public string? OtherStandards { get; set; }

    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; set; }

    [JsonPropertyName("meta_title")]
    public string? MetaTitle { get; set; }

    [JsonPropertyName("meta_description")]
    public string? MetaDescription { get; set; }

    [JsonPropertyName("status_id")]
    public string? StatusId { get; set; }

    [JsonPropertyName("cover_image_url")]
    public string? CoverImageUrl { get; set; }

    public int Stock { get; set; }

    [JsonPropertyName("has_been_booked")]
    public long HasBeenBooked { get; set; }

    [JsonPropertyName("status_stock_id")]
    public string StatusStockId { get; set; } = string.Empty;

    [JsonPropertyName("inventory_status")]
    public string InventoryStatus { get; set; } = Domain.Constants.InventoryStatus.InStock;

    public string? Highlights { get; set; }

    [JsonPropertyName("compatible_vehicle_model_ids")]
    public List<int> CompatibleVehicleModelIds { get; set; } = [];

    public List<ProductVariantDetailForManagerResponse> Variants { get; set; } = [];
}

