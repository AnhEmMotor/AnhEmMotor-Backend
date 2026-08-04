using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Features.Products.Queries.GetProductsList;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Services;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Products.Queries.GetPersonalizedRecommendations;

public class GetPersonalizedRecommendationsQueryHandler(
    IProductViewRepository productViewRepository,
    ICurrentUserContext currentUserContext,
    ISender sender) : IRequestHandler<GetPersonalizedRecommendationsQuery, Result<PagedResult<ProductListStoreResponse>>>
{
    private const int MaxHistoryRows = 200;
    private const int HistoryWindowDays = 90;
    private const double RecencyHalfLifeDays = 14;
    private const double MinDwellWeight = 0.1;
    private const double MaxDwellMinutes = 3.0;
    private const int TopCategoryCount = 3;

    public async Task<Result<PagedResult<ProductListStoreResponse>>> Handle(
        GetPersonalizedRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        var customerUserId = currentUserContext.GetUserIdOrNull();
        var topCategoryIds = new List<int>();
        if (customerUserId is not null || !string.IsNullOrWhiteSpace(request.VisitorKey))
        {
            var since = DateTimeOffset.UtcNow.AddDays(-HistoryWindowDays);
            var views = await productViewRepository.GetRecentViewsAsync(
                customerUserId,
                request.VisitorKey,
                since,
                MaxHistoryRows,
                cancellationToken);
            var now = DateTimeOffset.UtcNow;
            topCategoryIds = views
                .Where(v => v.CategoryId.HasValue)
                .GroupBy(v => v.CategoryId!.Value)
                .Select(
                    g => new
                    {
                        CategoryId = g.Key,
                        Weight = g.Sum(
                            v =>
                            {
                                var ageDays = (now - v.ViewedAt).TotalDays;
                                var recencyWeight = Math.Pow(0.5, ageDays / RecencyHalfLifeDays);
                                var dwellWeight = Math.Clamp(v.DwellTimeMs / 60000.0, MinDwellWeight, MaxDwellMinutes);
                                return recencyWeight * dwellWeight;
                            })
                    })
                .OrderByDescending(g => g.Weight)
                .Take(TopCategoryCount)
                .Select(g => g.CategoryId)
                .ToList();
        }
        return await sender.Send(
            new GetProductsListQuery { PageSize = request.PageSize, CategoryIds = topCategoryIds, Sorts = "-createdAt" },
            cancellationToken);
    }
}
