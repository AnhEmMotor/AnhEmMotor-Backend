using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Brands.Queries.GetBrandsList;

public sealed record GetBrandsListQuery : IRequest<Result<PagedResult<BrandResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
