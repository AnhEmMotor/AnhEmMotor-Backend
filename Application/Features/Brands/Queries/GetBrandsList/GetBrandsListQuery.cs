using Application.ApiContracts.Brand;
using Domain.Helpers;
using MediatR;
using Sieve.Models;

namespace Application.Features.Brands.Queries.GetBrandsList;

public sealed record GetBrandsListQuery(SieveModel SieveModel) : IRequest<PagedResult<BrandResponse>>;
