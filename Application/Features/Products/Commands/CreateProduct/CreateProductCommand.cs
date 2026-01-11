using Application.ApiContracts.Product.Requests;
using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand : IRequest<Result<ProductDetailForManagerResponse?>>
{
    public string? Name { get; init; }

    public int? CategoryId { get; init; }

    public int? BrandId { get; init; }

    public string? StatusId { get; init; } = "for-sale";

    public string? Description { get; init; }

    public decimal? Weight { get; init; }

    public string? Dimensions { get; init; }

    public string? Wheelbase { get; init; }

    public decimal? SeatHeight { get; init; }

    public decimal? GroundClearance { get; init; }

    public decimal? FuelCapacity { get; init; }

    public string? TireSize { get; init; }

    public string? FrontSuspension { get; init; }

    public string? RearSuspension { get; init; }

    public string? EngineType { get; init; }

    public string? MaxPower { get; init; }

    public decimal? OilCapacity { get; init; }

    public string? FuelConsumption { get; init; }

    public string? TransmissionType { get; init; }

    public string? StarterSystem { get; init; }

    public string? MaxTorque { get; init; }

    public decimal? Displacement { get; init; }

    public string? BoreStroke { get; init; }

    public string? CompressionRatio { get; init; }

    public List<ProductVariantWriteRequest> Variants { get; init; } = [];
}
