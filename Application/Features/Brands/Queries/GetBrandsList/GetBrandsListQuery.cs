using Application.ApiContracts.Brand.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.Brands.Queries.GetBrandsList;

public sealed record GetBrandsListQuery(SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<BrandResponse>>;
