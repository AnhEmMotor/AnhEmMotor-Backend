using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Brands.Commands.CreateBrand;

public sealed record CreateBrandCommand : IRequest<Result<BrandResponse>>
{
    public string? Name { get; init; }

    public string? Description { get; init; }
}
