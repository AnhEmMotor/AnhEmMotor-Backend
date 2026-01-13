using Application.ApiContracts.Brand.Responses;
using MediatR;
using Sieve.Models;
using Domain.Primitives;
using Application.Common.Models;

namespace Application.Features.Brands.Queries.GetDeletedBrandsList;

public sealed record GetDeletedBrandsListQuery : IRequest<Result<PagedResult<BrandResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
