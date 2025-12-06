using Domain.Shared;
using MediatR;
using Sieve.Models;

namespace Application.Features.Brands.Queries.GetDeletedBrandsList;

public sealed record GetDeletedBrandsListQuery(SieveModel SieveModel) : IRequest<PagedResult<ApiContracts.Brand.Responses.BrandResponse>>;
