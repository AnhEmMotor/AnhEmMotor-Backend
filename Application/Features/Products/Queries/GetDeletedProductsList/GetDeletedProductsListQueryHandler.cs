using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetDeletedProductsList;

public sealed class GetDeletedProductsListQueryHandler(IProductSelectRepository repository) : IRequestHandler<GetDeletedProductsListQuery, PagedResult<ProductDetailResponse>>
{
    public async Task<PagedResult<ProductDetailResponse>> Handle(GetDeletedProductsListQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetDeletedProducts()
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.VariantOptionValues)
                    .ThenInclude(vov => vov.OptionValue)
                        .ThenInclude(ov => ov!.Option)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.ProductCollectionPhotos)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.InputInfos)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.OutputInfos)
                    .ThenInclude(oi => oi.OutputOrder);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var products = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var responses = products.Select(ProductResponseMapper.BuildProductDetailResponse).ToList();

        return new PagedResult<ProductDetailResponse>(responses, totalCount, request.Page, request.PageSize);
    }
}
