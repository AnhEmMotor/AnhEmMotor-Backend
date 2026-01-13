using Application.ApiContracts.Product.Requests;
using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand : IRequest<Result<ProductDetailForManagerResponse?>>
{
    public int Id { get; init; }

    public string? Name { get; init; }

    [JsonPropertyName("category_id")]
    public int? CategoryId { get; init; }

    [JsonPropertyName("brand_id")]
    public int? BrandId { get; init; }

    public string? Description { get; init; }

    public decimal? Weight { get; init; }

    public string? Dimensions { get; init; }

    public string? Wheelbase { get; init; }

    [JsonPropertyName("seat_height")]
    public decimal? SeatHeight { get; init; }

    [JsonPropertyName("ground_clearance")]
    public decimal? GroundClearance { get; init; }

    [JsonPropertyName("fuel_capacity")]
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
    public decimal? OilCapacity { get; init; }

    [JsonPropertyName("fuel_consumption")]
    public string? FuelConsumption { get; init; }

    [JsonPropertyName("transmission_type")]
    public string? TransmissionType { get; init; }

    [JsonPropertyName("starter_system")]
    public string? StarterSystem { get; init; }

    [JsonPropertyName("max_torque")]
    public string? MaxTorque { get; init; }

    public decimal? Displacement { get; init; }

    [JsonPropertyName("bore_stroke")]
    public string? BoreStroke { get; init; }

    [JsonPropertyName("compression_ratio")]
    public string? CompressionRatio { get; init; }

    public List<UpdateProductVariantRequest> Variants { get; init; } = [];
}

