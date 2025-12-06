using Domain.Shared;
using MediatR;
using Sieve.Models;

namespace Application.Features.Brands.Queries.GetBrandsList;

public sealed record GetBrandsListQuery(SieveModel SieveModel) : IRequest<PagedResult<ApiContracts.Brand.Responses.BrandResponse>>;
