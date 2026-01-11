using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.ProductCategories.Queries.GetDeletedProductCategoriesList;

public sealed record GetDeletedProductCategoriesListQuery(SieveModel SieveModel) : IRequest<Result<PagedResult<ProductCategoryResponse>>>;
