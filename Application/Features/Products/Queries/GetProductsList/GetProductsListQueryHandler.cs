using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories.Product;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Domain.Entities.Product;

namespace Application.Features.Products.Queries.GetProductsList;

public sealed class GetProductsListQueryHandler(IProductSelectRepository selectRepository)
    : IRequestHandler<GetProductsListQuery, PagedResult<ProductDetailResponse>>
{
    public Task<PagedResult<ProductDetailResponse>> Handle(
        GetProductsListQuery request,
        CancellationToken cancellationToken)
    {
        return BuildProductPagedResultAsync(selectRepository.GetActiveProducts(), selectRepository.GetActiveVariants(), request, cancellationToken);
    }

    private static async Task<PagedResult<ProductDetailResponse>> BuildProductPagedResultAsync(
        IQueryable<ProductEntity> productSource,
        IQueryable<ProductVariant> variantSource,
        GetProductsListQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Max(request.PageSize, 1);
        var searchPattern = string.IsNullOrWhiteSpace(request.Search) ? null : $"%{request.Search.Trim()}%";
        var normalizedStatuses = ProductResponseMapper.NormalizeStatuses(request.StatusIds);

        var query = productSource
            .Include(p => p.Brand)
            .Include(p => p.ProductCategory)
            .Include(p => p.ProductVariants)
            .AsNoTracking();

        if (searchPattern != null)
        {
            query = query.Where(p =>
                (p.Name != null && EF.Functions.Like(p.Name, searchPattern)) ||
                (p.ProductCategory != null && p.ProductCategory.Name != null && EF.Functions.Like(p.ProductCategory.Name, searchPattern)) ||
                (p.Brand != null && p.Brand.Name != null && EF.Functions.Like(p.Brand.Name, searchPattern))
            );
        }

        if (normalizedStatuses.Count > 0)
        {
            query = query.Where(p => p.StatusId != null && normalizedStatuses.Contains(p.StatusId));
        }

        var projection = query.Select(p => new ProductListRow
        {
            Id = p.Id,
            Name = p.Name,
            CategoryId = p.CategoryId,
            CategoryName = p.ProductCategory != null ? p.ProductCategory.Name : null,
            BrandId = p.BrandId,
            BrandName = p.Brand != null ? p.Brand.Name : null,
            Description = p.Description,
            Weight = p.Weight,
            Dimensions = p.Dimensions,
            Wheelbase = p.Wheelbase,
            SeatHeight = p.SeatHeight,
            GroundClearance = p.GroundClearance,
            FuelCapacity = p.FuelCapacity,
            TireSize = p.TireSize,
            FrontSuspension = p.FrontSuspension,
            RearSuspension = p.RearSuspension,
            EngineType = p.EngineType,
            MaxPower = p.MaxPower,
            OilCapacity = p.OilCapacity,
            FuelConsumption = p.FuelConsumption,
            TransmissionType = p.TransmissionType,
            StarterSystem = p.StarterSystem,
            MaxTorque = p.MaxTorque,
            Displacement = p.Displacement,
            BoreStroke = p.BoreStroke,
            CompressionRatio = p.CompressionRatio,
            StatusId = p.StatusId,
            CreatedAt = EF.Property<DateTimeOffset?>(p, AuditingProperties.CreatedAt),
            TotalStock = p.ProductVariants.SelectMany(v => v.InputInfos).Sum(ii => ii.RemainingCount) ?? 0,
            TotalBooked = p.ProductVariants.SelectMany(v => v.OutputInfos)
                .Where(oi => oi.OutputOrder != null && OrderBookingStatuses.All.Contains(oi.OutputOrder.StatusId ?? string.Empty))
                .Sum(oi => (long?)oi.Count) ?? 0
        });

        var totalCount = await projection.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await projection
            .OrderBy(row => row.TotalStock - row.TotalBooked)
            .ThenBy(row => row.CreatedAt)
            .ThenBy(row => row.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var productIds = items.Select(item => item.Id).ToList();
        var variantRows = await variantSource.AsNoTracking()
            .Where(v => v.ProductId.HasValue && productIds.Contains(v.ProductId.Value))
            .Include(v => v.ProductCollectionPhotos)
            .Include("VariantOptionValues.OptionValue.Option")
            .Include(v => v.InputInfos)
            .Include("OutputInfos.OutputOrder")
            .Select(v => new VariantRow
            {
                Id = v.Id,
                ProductId = v.ProductId ?? 0,
                UrlSlug = v.UrlSlug,
                Price = v.Price,
                CoverImageUrl = v.CoverImageUrl,
                Photos = v.ProductCollectionPhotos.Select(photo => photo.ImageUrl ?? string.Empty).ToList(),
                OptionPairs = v.VariantOptionValues.Select(vov => new OptionPair
                {
                    OptionName = vov.OptionValue != null && vov.OptionValue.Option != null ? vov.OptionValue.Option.Name : null,
                    OptionValue = vov.OptionValue != null ? vov.OptionValue.Name : null
                }).ToList(),
                Stock = v.InputInfos.Sum(ii => ii.RemainingCount) ?? 0,
                HasBeenBooked = v.OutputInfos
                    .Where(oi => oi.OutputOrder != null && OrderBookingStatuses.All.Contains(oi.OutputOrder.StatusId ?? string.Empty))
                    .Sum(oi => (long?)oi.Count) ?? 0
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var variantLookup = variantRows.GroupBy(row => row.ProductId).ToDictionary(group => group.Key, group => group.ToList());
        var responses = new List<ProductDetailResponse>(items.Count);
        foreach (var item in items)
        {
            var variants = variantLookup.TryGetValue(item.Id, out var rows) ? rows : [];
            responses.Add(ProductResponseMapper.BuildProductDetailResponse(item, variants));
        }

        return new PagedResult<ProductDetailResponse>(responses, totalCount, page, pageSize);
    }
}
