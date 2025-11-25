using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories.Product;
using Domain.Enums;
using Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetProductsList;

public sealed class GetProductsListQueryHandler(IProductReadRepository readRepository)
    : IRequestHandler<GetProductsListQuery, PagedResult<ProductDetailResponse>>
{
    public async Task<PagedResult<ProductDetailResponse>> Handle(
        GetProductsListQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Max(request.PageSize, 1);
        var searchPattern = string.IsNullOrWhiteSpace(request.Search) ? null : $"%{request.Search.Trim()}%";
        var normalizedStatuses = ProductResponseMapper.NormalizeStatuses(request.StatusIds);

        var query = readRepository.GetQueryable(DataFetchMode.ActiveOnly)
            .AsNoTracking();

        if (searchPattern != null)
        {
            query = query.Where(p =>
                EF.Functions.Like(p.Name, searchPattern) ||
                (p.ProductCategory != null && EF.Functions.Like(p.ProductCategory.Name, searchPattern)) ||
                (p.Brand != null && EF.Functions.Like(p.Brand.Name, searchPattern))
            );
        }

        if (normalizedStatuses.Count > 0)
        {
            query = query.Where(p => p.StatusId != null && normalizedStatuses.Contains(p.StatusId));
        }

        // 3. Count Total Items (Efficient Count)
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            // Include Variants to calculate Stock. 
            // WARNING: If a product has 1000 variants, this is heavy. 
            // But for product list, usually variants are few.
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.InputInfos)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.OutputInfos)
                    .ThenInclude(oi => oi.OutputOrder)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.ProductCollectionPhotos)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.VariantOptionValues)
                    .ThenInclude(vov => vov.OptionValue)
                        .ThenInclude(ov => ov!.Option)
            .OrderByDescending(p => p.CreatedAt) // Default Sort
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery() // Important for performance with many includes
            .ToListAsync(cancellationToken);

        // 5. Map to Response (In-Memory)
        var responses = items.Select(item =>
            ProductResponseMapper.BuildProductDetailResponse(item) // Helper mapper should handle logic
        ).ToList();

        return new PagedResult<ProductDetailResponse>(responses, totalCount, page, pageSize);
    }
}