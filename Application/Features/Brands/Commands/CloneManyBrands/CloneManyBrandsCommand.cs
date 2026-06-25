using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Brands.Commands.CloneManyBrands;

public sealed record CloneManyBrandsCommand : IRequest<Result<List<BrandResponse>>>
{
    public List<int> Ids { get; init; } = [];
}
