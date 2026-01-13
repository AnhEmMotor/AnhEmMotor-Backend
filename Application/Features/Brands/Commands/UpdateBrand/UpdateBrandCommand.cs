
using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Brands.Commands.UpdateBrand;

public sealed record UpdateBrandCommand : IRequest<Result<BrandResponse?>>
{
    public int Id { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }
}
