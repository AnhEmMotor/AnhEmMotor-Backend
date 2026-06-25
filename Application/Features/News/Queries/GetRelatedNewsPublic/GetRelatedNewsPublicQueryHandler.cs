using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.News;
using MediatR;
using Sieve.Models;
using System.Linq.Expressions;

namespace Application.Features.News.Queries.GetRelatedNewsPublic;

public class GetRelatedNewsPublicQueryHandler(INewsReadRepository repository) : IRequestHandler<GetRelatedNewsPublicQuery, Result<List<NewsSummaryResponse>>>
{
    public async Task<Result<List<NewsSummaryResponse>>> Handle(
        GetRelatedNewsPublicQuery request,
        CancellationToken cancellationToken)
    {
        var targetNews = await repository.GetBySlugAsync(request.Slug, cancellationToken).ConfigureAwait(false);
        if (targetNews == null || targetNews.LinkedProducts == null || !targetNews.LinkedProducts.Any())
        {
            return new List<NewsSummaryResponse>();
        }
        var variantIds = targetNews.LinkedProducts.Select(lp => lp.ProductVariantId).Distinct().ToList();
        var colorIds = targetNews.LinkedProducts
            .Where(lp => lp.ProductVariantColorId.HasValue)
            .Select(lp => lp.ProductVariantColorId!.Value)
            .Distinct()
            .ToList();
        Expression<Func<Domain.Entities.News, bool>> filter = x => x.IsPublished &&
            x.Id != targetNews.Id &&
            x.LinkedProducts
                .Any(
                    lp => variantIds.Contains(lp.ProductVariantId) ||
                            (lp.ProductVariantColorId.HasValue && colorIds.Contains(lp.ProductVariantColorId.Value)));
        var sieveModel = new SieveModel { Page = 1, PageSize = 5, Sorts = "-PublishedDate" };
        var result = await repository.GetPagedAsync<NewsSummaryResponse>(
            sieveModel,
            filter: filter,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result.Items?.ToList() ?? [];
    }
}
