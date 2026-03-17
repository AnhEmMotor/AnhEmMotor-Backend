using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using Domain.Primitives;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetDeletedProductsList;

public sealed class GetDeletedProductsListQueryHandler(IProductReadRepository repository) : IRequestHandler<GetDeletedProductsListQuery, Result<PagedResult<ProductDetailForManagerResponse>>>
{
    public async Task<Result<PagedResult<ProductDetailForManagerResponse>>> Handle(
        GetDeletedProductsListQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var pagedResult = await repository.GetPagedDeletedProductsAsync(
            page,
            pageSize,
            request.Filters,
            request.Sorts,
            cancellationToken)
            .ConfigureAwait(false);

        var products = pagedResult.Items;
        var totalCount = pagedResult.TotalCount;

        var responses = products.Select(p => p.Adapt<ProductDetailForManagerResponse>()).ToList();

        return new PagedResult<ProductDetailForManagerResponse>(responses, totalCount, page, pageSize);
    }
}