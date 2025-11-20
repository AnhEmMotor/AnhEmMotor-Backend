using Application.ApiContracts.Brand;
using Domain.Helpers;
using MediatR;
using Sieve.Models;

namespace Application.Features.Brands.Queries.GetDeletedBrandsList;

public sealed record GetDeletedBrandsListQuery(SieveModel SieveModel) : IRequest<PagedResult<BrandResponse>>;
