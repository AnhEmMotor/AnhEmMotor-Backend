
using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Brands.Commands.RestoreManyBrands;

public sealed record RestoreManyBrandsCommand : IRequest<Result<List<BrandResponse>?>>
{
    public List<int> Ids { get; init; } = [];
}
