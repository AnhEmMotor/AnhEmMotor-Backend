using Application.ApiContracts.Product.Requests;
using Application.ApiContracts.Product.Responses;
using Application.Common.Converters;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand : IRequest<Result<ProductDetailForManagerResponse?>>
{
    public string? Name { get; init; }

    [JsonPropertyName("category_id")]
    public int? CategoryId { get; init; }

    [JsonPropertyName("brand_id")]
    public int? BrandId { get; init; }

    [JsonPropertyName("status_id")]
    public string? StatusId { get; init; } = "for-sale";

    public string? Description { get; init; }

    [JsonConverter(typeof(NullableDecimalConverter))]
    public decimal? Weight { get; init; }

    public string? Dimensions { get; init; }

    [JsonConverter(typeof(NullableDecimalConverter))]
    public decimal? Wheelbase { get; init; }

    [JsonPropertyName("seat_height")]
    [JsonConverter(typeof(NullableDecimalConverter))]
    public decimal? SeatHeight { get; init; }

    [JsonPropertyName("ground_clearance")]
    [JsonConverter(typeof(NullableDecimalConverter))]
    public decimal? GroundClearance { get; init; }

    [JsonPropertyName("fuel_capacity")]
    [JsonConverter(typeof(NullableDecimalConverter))]
    public decimal? FuelCapacity { get; init; }

    [JsonPropertyName("tire_size")]
    public string? TireSize { get; init; }

    [JsonPropertyName("front_suspension")]
    public string? FrontSuspension { get; init; }

    [JsonPropertyName("rear_suspension")]
    public string? RearSuspension { get; init; }

    [JsonPropertyName("engine_type")]
    public string? EngineType { get; init; }

    [JsonPropertyName("max_power")]
    public string? MaxPower { get; init; }

    [JsonPropertyName("oil_capacity")]
    [JsonConverter(typeof(NullableDecimalConverter))]
    public decimal? OilCapacity { get; init; }

    [JsonPropertyName("fuel_consumption")]
    public string? FuelConsumption { get; init; }

    [JsonPropertyName("transmission_type")]
    public string? TransmissionType { get; init; }

    [JsonPropertyName("starter_system")]
    public string? StarterSystem { get; init; }

    [JsonPropertyName("max_torque")]
    public string? MaxTorque { get; init; }

    [JsonConverter(typeof(NullableDecimalConverter))]
    public decimal? Displacement { get; init; }

    [JsonPropertyName("bore_stroke")]
    public string? BoreStroke { get; init; }

    [JsonPropertyName("compression_ratio")]
    public string? CompressionRatio { get; init; }

    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; init; }

    [JsonPropertyName("meta_title")]
    public string? MetaTitle { get; init; }

    [JsonPropertyName("meta_description")]
    public string? MetaDescription { get; init; }

    [JsonPropertyName("highlights")]
    public string? Highlights { get; init; }

    public List<CreateProductVariantRequest> Variants { get; init; } = [];
}
