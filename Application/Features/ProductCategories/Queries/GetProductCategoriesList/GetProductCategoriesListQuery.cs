using Application.ApiContracts.ProductCategory.Responses;
using Domain.Shared;
using MediatR;
using Sieve.Models;

namespace Application.Features.ProductCategories.Queries.GetProductCategoriesList;

public sealed record GetProductCategoriesListQuery(SieveModel SieveModel) : IRequest<PagedResult<ProductCategoryResponse>>;
