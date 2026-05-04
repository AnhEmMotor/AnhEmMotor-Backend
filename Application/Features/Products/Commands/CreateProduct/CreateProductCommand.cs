using Application.ApiContracts.Product.Requests;
using Application.ApiContracts.Product.Responses;
using Application.Common.Converters;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand : IRequest<Result<ProductDetailForManagerResponse?>>
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("categoryId")]
    public int? CategoryId { get; init; }

    [JsonPropertyName("brandId")]
    public int? BrandId { get; init; }

    public string? StatusId { get; init; } = "for-sale";

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("weight")]
    [JsonConverter(typeof(NullableDecimalConverter))]
    public decimal? Weight { get; init; }

    [JsonPropertyName("dimensions")]
    public string? Dimensions { get; init; }

    [JsonPropertyName("wheelbase")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Wheelbase { get; init; }

    [JsonPropertyName("seatHeight")]
    [JsonConverter(typeof(NullableDecimalConverter))]
    public decimal? SeatHeight { get; init; }

    [JsonPropertyName("groundClearance")]
    [JsonConverter(typeof(NullableDecimalConverter))]
    public decimal? GroundClearance { get; init; }

    [JsonPropertyName("fuelCapacity")]
    [JsonConverter(typeof(NullableDecimalConverter))]
    public decimal? FuelCapacity { get; init; }

    [JsonPropertyName("tireSize")]
    public string? TireSize { get; init; }

    [JsonPropertyName("frontSuspension")]
    public string? FrontSuspension { get; init; }

    [JsonPropertyName("rearSuspension")]
    public string? RearSuspension { get; init; }

    [JsonPropertyName("engineType")]
    public string? EngineType { get; init; }

    [JsonPropertyName("maxPower")]
    public string? MaxPower { get; init; }

    [JsonPropertyName("oilCapacity")]
    [JsonConverter(typeof(NullableDecimalConverter))]
    public decimal? OilCapacity { get; init; }

    [JsonPropertyName("fuelConsumption")]
    public string? FuelConsumption { get; init; }

    [JsonPropertyName("transmissionType")]
    public string? TransmissionType { get; init; }

    [JsonPropertyName("starterSystem")]
    public string? StarterSystem { get; init; }

    [JsonPropertyName("maxTorque")]
    public string? MaxTorque { get; init; }

    [JsonPropertyName("displacement")]
    [JsonConverter(typeof(NullableDecimalConverter))]
    public decimal? Displacement { get; init; }

    [JsonPropertyName("boreStroke")]
    public string? BoreStroke { get; init; }

    [JsonPropertyName("compressionRatio")]
    public string? CompressionRatio { get; init; }

    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; init; }

    [JsonPropertyName("meta_title")]
    public string? MetaTitle { get; init; }

    [JsonPropertyName("meta_description")]
    public string? MetaDescription { get; init; }

    [JsonPropertyName("variants")]
    public List<CreateProductVariantRequest> Variants { get; init; } = [];
}
