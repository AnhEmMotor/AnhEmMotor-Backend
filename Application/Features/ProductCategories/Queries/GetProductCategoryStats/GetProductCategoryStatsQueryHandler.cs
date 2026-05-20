using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ProductCategory;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.ProductCategories.Queries.GetProductCategoryStats;

public sealed class GetProductCategoryStatsQueryHandler(IProductCategoryReadRepository productCategoryRepository) : IRequestHandler<GetProductCategoryStatsQuery, Result<ProductCategoryStatsResponse>>
{
    public async Task<Result<ProductCategoryStatsResponse>> Handle(
        GetProductCategoryStatsQuery request,
        CancellationToken cancellationToken)
    {
        var stats = await productCategoryRepository.GetStatisticsAsync(cancellationToken).ConfigureAwait(false);
        return stats;
    }
}
