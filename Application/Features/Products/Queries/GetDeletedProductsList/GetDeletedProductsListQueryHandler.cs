using Application.Interfaces.Repositories.Product;
using Domain.Shared;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetDeletedProductsList;

public sealed class GetDeletedProductsListQueryHandler(IProductReadRepository repository) : IRequestHandler<GetDeletedProductsListQuery, PagedResult<ApiContracts.Product.Responses.ProductDetailResponse>>
{
    public async Task<PagedResult<ApiContracts.Product.Responses.ProductDetailResponse>> Handle(
        GetDeletedProductsListQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var (products, totalCount) = await repository.GetPagedDeletedProductsAsync(page, pageSize, cancellationToken)
            .ConfigureAwait(false);

        var responses = products.Select(p => p.Adapt<ApiContracts.Product.Responses.ProductDetailResponse>()).ToList();

        return new PagedResult<ApiContracts.Product.Responses.ProductDetailResponse>(
            responses,
            totalCount,
            page,
            pageSize);
    }
}