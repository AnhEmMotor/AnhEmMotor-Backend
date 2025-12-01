using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Common;

public abstract class ProductWriteRequestBase
{
    public string? Name { get; set; }

    [JsonPropertyName("category_id")]
    public int? CategoryId { get; set; }

    [JsonPropertyName("brand_id")]
    public int? BrandId { get; set; }

    public string? Description { get; set; }

    public decimal? Weight { get; set; }

    public string? Dimensions { get; set; }

    public string? Wheelbase { get; set; }

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

    public List<ProductVariantWriteRequest> Variants { get; set; } = [];
}
