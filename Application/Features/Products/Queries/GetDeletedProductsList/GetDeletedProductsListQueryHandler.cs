using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories.Product;
using Domain.Enums;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetDeletedProductsList;

public sealed class GetDeletedProductsListQueryHandler(IProductSelectRepository repository) : IRequestHandler<GetDeletedProductsListQuery, PagedResult<ProductDetailResponse>>
{
    public async Task<PagedResult<ProductDetailResponse>> Handle(GetDeletedProductsListQuery request, CancellationToken cancellationToken)
    {
        string deletedAtProp = AuditingProperties.DeletedAt;
        var query = repository.GetDeletedProducts()
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.VariantOptionValues.Where(v => EF.Property<DateTimeOffset?>(v, deletedAtProp) == null))
                    .ThenInclude(vov => vov.OptionValue)
                        .ThenInclude(ov => ov!.Option)
            .Include(p => p.ProductVariants.Where(v => EF.Property<DateTimeOffset?>(v, deletedAtProp) == null))
                .ThenInclude(v => v.ProductCollectionPhotos)
            .Include(p => p.ProductVariants.Where(v => EF.Property<DateTimeOffset?>(v, deletedAtProp) == null))
                .ThenInclude(v => v.InputInfos)
            .Include(p => p.ProductVariants.Where(v => EF.Property<DateTimeOffset?>(v, deletedAtProp) == null))
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
