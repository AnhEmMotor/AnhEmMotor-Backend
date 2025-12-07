using Application.ApiContracts.Brand.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.Brands.Queries.GetDeletedBrandsList;

public sealed record GetDeletedBrandsListQuery(SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<BrandResponse>>;
