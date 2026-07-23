using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Primitives;
using Mapster;

using MediatR;

namespace Application.Features.Products.Queries.GetActiveVariantLiteListForManager;

public class GetActiveVariantLiteListForManagerQueryHandler(
    IProductVariantReadRepository repository,
    IFileReadService fileReadService) : IRequestHandler<GetActiveVariantLiteListForManagerQuery, Result<PagedResult<ProductVariantLiteResponse>>>
{
    public async Task<Result<PagedResult<ProductVariantLiteResponse>>> Handle(
        GetActiveVariantLiteListForManagerQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Max(request.PageSize, 1);
        var (variants, totalCount) = await repository.GetPagedVariantsAsync(
            page,
            pageSize,
            request.Filters,
            request.Sorts,
            cancellationToken,
            search: request.Search)
            .ConfigureAwait(false);
        var responses = variants.Select(v => v.Adapt<ProductVariantLiteResponse>()).ToList();
        foreach (var response in responses)
        {
            if (!string.IsNullOrWhiteSpace(response.CoverImageUrl) &&
                !response.CoverImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                response.CoverImageUrl = fileReadService.GetPublicUrl(response.CoverImageUrl);
            }
            if (response.Photos != null)
            {
                for (int i = 0; i < response.Photos.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(response.Photos[i]) &&
                        !response.Photos[i].StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        response.Photos[i] = fileReadService.GetPublicUrl(response.Photos[i]);
                    }
                }
            }
        }
        return new PagedResult<ProductVariantLiteResponse>(responses, totalCount, page, pageSize);
    }
}
