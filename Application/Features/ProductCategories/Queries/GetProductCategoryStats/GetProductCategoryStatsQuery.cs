using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.ProductCategories.Queries.GetProductCategoryStats;

public sealed record GetProductCategoryStatsQuery : IRequest<Result<ProductCategoryStatsResponse>>
{
}
