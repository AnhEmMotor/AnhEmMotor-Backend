using Application.ApiContracts.ProductCategory;
using Domain.Shared;
using MediatR;
using Sieve.Models;

namespace Application.Features.ProductCategories.Queries.GetDeletedProductCategoriesList;

public sealed record GetDeletedProductCategoriesListQuery(SieveModel SieveModel) : IRequest<PagedResult<ProductCategoryResponse>>;
