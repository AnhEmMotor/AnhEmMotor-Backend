using Application.ApiContracts.Product.Requests;
using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand : IRequest<Result<ProductDetailForManagerResponse?>>
{
    public string? Name { get; init; }

    public int? CategoryId { get; init; }

    public int? BrandId { get; init; }

    public string? StatusId { get; init; } = "for-sale";

    public string? Description { get; init; }

    [JsonConverter(typeof(Common.Converters.NullableDecimalConverter))]
    public decimal? Weight { get; init; }

    public string? Dimensions { get; init; }

    public string? Wheelbase { get; init; }

    [JsonConverter(typeof(Common.Converters.NullableDecimalConverter))]
    public decimal? SeatHeight { get; init; }

    [JsonConverter(typeof(Common.Converters.NullableDecimalConverter))]
    public decimal? GroundClearance { get; init; }

    [JsonConverter(typeof(Common.Converters.NullableDecimalConverter))]
    public decimal? FuelCapacity { get; init; }

    public string? TireSize { get; init; }

    public string? FrontSuspension { get; init; }

    public string? RearSuspension { get; init; }

    public string? EngineType { get; init; }

    public string? MaxPower { get; init; }

    [JsonConverter(typeof(Common.Converters.NullableDecimalConverter))]
    public decimal? OilCapacity { get; init; }

    public string? FuelConsumption { get; init; }

    public string? TransmissionType { get; init; }

    public string? StarterSystem { get; init; }

    public string? MaxTorque { get; init; }

    [JsonConverter(typeof(Common.Converters.NullableDecimalConverter))]
    public decimal? Displacement { get; init; }

    public string? BoreStroke { get; init; }

    public string? CompressionRatio { get; init; }
    
    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; init; }
    
    [JsonPropertyName("meta_title")]
    public string? MetaTitle { get; init; }
    
    [JsonPropertyName("meta_description")]
    public string? MetaDescription { get; init; }

    public List<CreateProductVariantRequest> Variants { get; init; } = [];
}
