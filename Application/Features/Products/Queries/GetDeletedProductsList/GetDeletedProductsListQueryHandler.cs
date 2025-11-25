using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories.Product;
using Domain.Shared;
using MediatR;

namespace Application.Features.Products.Queries.GetDeletedProductsList;

public sealed class GetDeletedProductsListQueryHandler(IProductReadRepository repository)
    : IRequestHandler<GetDeletedProductsListQuery, PagedResult<ProductDetailResponse>>
{
    public async Task<PagedResult<ProductDetailResponse>> Handle(GetDeletedProductsListQuery request, CancellationToken cancellationToken)
    {
        // Validate Pagination Inputs
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        // Call Repository
        var (products, totalCount) = await repository.GetPagedDeletedProductsAsync(page, pageSize, cancellationToken);

        // Map Response
        var responses = products.Select(ProductResponseMapper.BuildProductDetailResponse).ToList();

        return new PagedResult<ProductDetailResponse>(responses, totalCount, page, pageSize);
    }
}