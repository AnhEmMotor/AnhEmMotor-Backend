using Application.ApiContracts.ProductCategory.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.ProductCategories.Queries.GetProductCategoriesList;

public sealed record GetProductCategoriesListQuery(SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<ProductCategoryResponse>>;
