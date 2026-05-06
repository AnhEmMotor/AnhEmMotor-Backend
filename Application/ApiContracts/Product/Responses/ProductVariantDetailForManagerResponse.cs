using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Responses;

public class ProductVariantDetailForManagerResponse
{
    public int? Id { get; set; }

    [JsonPropertyName("product_id")]
    public int? ProductId { get; set; }

    [JsonPropertyName("url")]
    public string? UrlSlug { get; set; }

    public decimal? Price { get; set; }

    [JsonPropertyName("cover_image_url")]
    public string? CoverImageUrl { get; set; }

    [JsonPropertyName("optionValues")]
    public Dictionary<string, string> OptionValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("version_name")]
    public string? VersionName { get; set; }

    [JsonPropertyName("color_name")]
    public string? ColorName { get; set; }

    [JsonPropertyName("color_code")]
    public string? ColorCode { get; set; }

    [JsonPropertyName("sku")]
    public string? SKU { get; set; }

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

    [JsonPropertyName("front_brake")]
    public string? FrontBrake { get; set; }

    [JsonPropertyName("rear_brake")]
    public string? RearBrake { get; set; }

    [JsonPropertyName("front_suspension")]
    public string? FrontSuspension { get; set; }

    [JsonPropertyName("rear_suspension")]
    public string? RearSuspension { get; set; }

    [JsonPropertyName("engine_type")]
    public string? EngineType { get; set; }

    [JsonPropertyName("stock_quantity")]
    public int? StockQuantity { get; set; }

    [JsonPropertyName("photo_collection")]
    public List<string> PhotoCollection { get; set; } = [];

    public int Stock { get; set; }

    [JsonPropertyName("has_been_booked")]
    public int HasBeenBooked { get; set; }

    [JsonPropertyName("status_stock_id")]
    public string StatusStockId { get; set; } = string.Empty;

    [JsonPropertyName("inventory_status")]
    public string InventoryStatus { get; set; } = Domain.Constants.InventoryStatus.InStock;
}

