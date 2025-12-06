using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.RestoreManyBrands;

public sealed record RestoreManyBrandsCommand : IRequest<(List<ApiContracts.Brand.Responses.BrandResponse>? Data, ErrorResponse? Error)>
{
    public List<int> Ids { get; init; } = [];
}
