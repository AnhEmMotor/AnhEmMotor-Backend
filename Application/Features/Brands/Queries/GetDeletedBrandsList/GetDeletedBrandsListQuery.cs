using Application.ApiContracts.Brand.Responses;
using MediatR;
using Sieve.Models;
using Domain.Primitives;
using Application.Common.Models;

namespace Application.Features.Brands.Queries.GetDeletedBrandsList;

public sealed record GetDeletedBrandsListQuery(SieveModel SieveModel) : IRequest<Result<PagedResult<BrandResponse>>>;
