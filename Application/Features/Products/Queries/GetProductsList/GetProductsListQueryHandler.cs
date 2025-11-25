using Application.ApiContracts.Product;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories.Product;
using Domain.Shared;
using MediatR;

namespace Application.Features.Products.Queries.GetProductsList;

public sealed class GetProductsListQueryHandler(IProductReadRepository readRepository) : IRequestHandler<GetProductsListQuery, PagedResult<ProductDetailResponse>>
{
    public async Task<PagedResult<ProductDetailResponse>> Handle(
        GetProductsListQuery request,
        CancellationToken cancellationToken)
    {
        var filter = new ProductFilter
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Search = request.Search,
            StatusIds = ProductResponseMapper.NormalizeStatuses(request.StatusIds)
        };

        var (items, totalCount) = await readRepository.GetPagedProductDetailsAsync(filter, cancellationToken);

        return new PagedResult<ProductDetailResponse>(items, totalCount, filter.Page, filter.PageSize);
    }
}