using Domain.Common.Models;
using MediatR;

namespace Application.Features.Brands.Commands.RestoreManyBrands;

public sealed record RestoreManyBrandsCommand : IRequest<(List<ApiContracts.Brand.Responses.BrandResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public List<int> Ids { get; init; } = [];
}
