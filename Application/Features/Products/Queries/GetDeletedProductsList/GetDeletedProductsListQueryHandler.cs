using Application.ApiContracts.Product.Responses;
using Application.Interfaces.Repositories.Product;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetDeletedProductsList;

public sealed class GetDeletedProductsListQueryHandler(IProductReadRepository repository) : IRequestHandler<GetDeletedProductsListQuery, Domain.Primitives.PagedResult<ProductDetailForManagerResponse>>
{
    public async Task<Domain.Primitives.PagedResult<ProductDetailForManagerResponse>> Handle(
        GetDeletedProductsListQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var (products, totalCount) = await repository.GetPagedDeletedProductsAsync(page, pageSize, cancellationToken)
            .ConfigureAwait(false);

        var responses = products.Select(p => p.Adapt<ProductDetailForManagerResponse>()).ToList();

        return new Domain.Primitives.PagedResult<ProductDetailForManagerResponse>(responses, totalCount, page, pageSize);
    }
}