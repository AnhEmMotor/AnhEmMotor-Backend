using Application.ApiContracts.Product;
using Application.Interfaces.Repositories.Product;
using Domain.Shared;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetDeletedProductsList;

public sealed class GetDeletedProductsListQueryHandler(IProductReadRepository repository) : IRequestHandler<GetDeletedProductsListQuery, PagedResult<ProductDetailResponse>>
{
    public async Task<PagedResult<ProductDetailResponse>> Handle(
        GetDeletedProductsListQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var (products, totalCount) = await repository.GetPagedDeletedProductsAsync(page, pageSize, cancellationToken);

        var responses = products.Select(p => p.Adapt<ProductDetailResponse>()).ToList();

        return new PagedResult<ProductDetailResponse>(responses, totalCount, page, pageSize);
    }
}