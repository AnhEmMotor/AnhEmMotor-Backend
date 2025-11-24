using Application.ApiContracts.Brand;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.RestoreManyBrands;

public sealed record RestoreManyBrandsCommand : IRequest<(List<BrandResponse>? Data, ErrorResponse? Error)>
{
    public List<int> Ids { get; init; } = [];
}
