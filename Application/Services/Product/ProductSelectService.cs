using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Get;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Services.Product;
using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Product;

public class ProductSelectService(IProductSelectRepository selectRepository) : IProductSelectService
{
    private static readonly string[] BookingStatuses =
    [
        "confirmed_cod",
            "paid_processing",
            "waiting_deposit",
            "deposit_paid",
            "delivering",
            "waiting_pickup"
    ];

    public Task<PagedResult<ProductDetailResponse>> GetProductsAsync(ProductListRequest request, CancellationToken cancellationToken)
    {
        return BuildProductPagedResultAsync(
            selectRepository.GetActiveProducts(),
            selectRepository.GetActiveVariants(),
            request,
            cancellationToken);
    }

    public Task<PagedResult<ProductDetailResponse>> GetDeletedProductsAsync(ProductListRequest request, CancellationToken cancellationToken)
    {
        return BuildProductPagedResultAsync(
            selectRepository.GetDeletedProducts(),
            selectRepository.GetDeletedVariants(),
            request,
            cancellationToken);
    }

    public Task<PagedResult<ProductVariantLiteResponse>> GetActiveVariantLiteProductsAsync(ProductListRequest request, CancellationToken cancellationToken)
    {
        return GetVariantLiteProductsInternalAsync(request, includeDeleted: false, cancellationToken);
    }

    public Task<PagedResult<ProductVariantLiteResponse>> GetDeletedVariantLiteProductsAsync(ProductListRequest request, CancellationToken cancellationToken)
    {
        return GetVariantLiteProductsInternalAsync(request, includeDeleted: true, cancellationToken);
    }

    private async Task<PagedResult<ProductVariantLiteResponse>> GetVariantLiteProductsInternalAsync(ProductListRequest request, bool includeDeleted, CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page ?? 1, 1);
        var pageSize = Math.Max(request.PageSize ?? 5, 1);
        var searchPattern = string.IsNullOrWhiteSpace(request.Search) ? null : $"%{request.Search.Trim()}%";
        var normalizedStatuses = NormalizeStatuses(request.StatusIds);
        if (!includeDeleted && normalizedStatuses.Count == 0)
        {
            normalizedStatuses.Add("for-sale");
        }

        var variantSource = includeDeleted ? selectRepository.GetDeletedVariants() : selectRepository.GetActiveVariants();
        var query = variantSource
            .Include("Product.ProductCategory")
            .Include(v => v.InputInfos)
            .Include("OutputInfos.OutputOrder")
            .AsNoTracking();

        if (searchPattern != null)
        {
            query = query.Where(v =>
                (v.Product != null && v.Product.Name != null && EF.Functions.Like(v.Product.Name, searchPattern)) ||
                (v.Product != null && v.Product.ProductCategory != null && v.Product.ProductCategory.Name != null && EF.Functions.Like(v.Product.ProductCategory.Name, searchPattern)) ||
                v.VariantOptionValues.Any(vov =>
                    (vov.OptionValue != null && vov.OptionValue.Name != null && EF.Functions.Like(vov.OptionValue.Name, searchPattern)) ||
                    (vov.OptionValue != null && vov.OptionValue.Option != null && vov.OptionValue.Option.Name != null && EF.Functions.Like(vov.OptionValue.Option.Name, searchPattern))
                ))
                .Include("VariantOptionValues.OptionValue.Option");
        }
        else
        {
            query = query.Include("VariantOptionValues.OptionValue.Option");
        }

        if (normalizedStatuses.Count > 0)
        {
            query = query.Where(v => v.Product != null && v.Product.StatusId != null && normalizedStatuses.Contains(v.Product.StatusId));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var variants = await query
            .OrderBy(v => CalculateAvailableStock(v))
            .ThenBy(v => EF.Property<DateTimeOffset?>(v.Product!, AuditingProperties.CreatedAt))
            .ThenBy(v => v.Product!.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var responses = variants.Select(variant =>
        {
            var variantName = BuildVariantName(variant.VariantOptionValues.Select(vov => new OptionPair
            {
                OptionName = vov.OptionValue?.Option?.Name,
                OptionValue = vov.OptionValue?.Name
            }));
            var displayName = string.IsNullOrWhiteSpace(variantName)
                ? variant.Product?.Name ?? string.Empty
                : $"{variant.Product?.Name} ({variantName})";

            return new ProductVariantLiteResponse
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                ProductName = variant.Product?.Name,
                VariantName = variantName,
                DisplayName = displayName,
                Price = variant.Price,
                CoverImageUrl = variant.CoverImageUrl,
                Stock = CalculateAvailableStock(variant)
            };
        }).ToList();

        return new PagedResult<ProductVariantLiteResponse>(responses, totalCount, page, pageSize);
    }

    public async Task<(ProductDetailResponse? Data, ErrorResponse? Error)> GetProductDetailsByIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var product = await selectRepository.GetProductWithDetailsByIdAsync(id, includeDeleted, cancellationToken).ConfigureAwait(false);
        if (product == null)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = $"Product with Id {id} not found." }] });
        }

        var inventoryAlertLevel = await selectRepository.GetInventoryAlertLevelAsync(cancellationToken).ConfigureAwait(false);
        var response = BuildProductDetailResponse(product, inventoryAlertLevel);
        return (response, null);
    }

    public async Task<(List<ProductVariantLiteResponse> Variants, ErrorResponse? Error)> GetVariantLiteByProductIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var product = await selectRepository.GetProductWithDetailsByIdAsync(id, includeDeleted, cancellationToken).ConfigureAwait(false);
        if (product == null)
        {
            return (new List<ProductVariantLiteResponse>(), new ErrorResponse { Errors = [new ErrorDetail { Message = $"Product with Id {id} not found." }] });
        }

        var variants = product.ProductVariants.Select(variant =>
        {
            var variantName = BuildVariantName(variant.VariantOptionValues.Select(vov => new OptionPair
            {
                OptionName = vov.OptionValue?.Option?.Name,
                OptionValue = vov.OptionValue?.Name
            }));

            return new ProductVariantLiteResponse
            {
                Id = variant.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                VariantName = variantName,
                DisplayName = string.IsNullOrWhiteSpace(variantName) ? product.Name ?? string.Empty : $"{product.Name} ({variantName})",
                Price = variant.Price,
                CoverImageUrl = variant.CoverImageUrl,
                Stock = CalculateAvailableStock(variant)
            };
        }).ToList();

        return (variants, null);
    }

    public async Task<SlugAvailabilityResponse> CheckSlugAvailabilityAsync(string slug, CancellationToken cancellationToken)
    {
        var normalizedSlug = slug?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedSlug))
        {
            return new SlugAvailabilityResponse { Slug = normalizedSlug, IsAvailable = false };
        }

        var existing = await selectRepository.GetVariantBySlugAsync(normalizedSlug, includeDeleted: true, cancellationToken).ConfigureAwait(false);
        return new SlugAvailabilityResponse
        {
            Slug = normalizedSlug,
            IsAvailable = existing == null
        };
    }

    private async Task<PagedResult<ProductDetailResponse>> BuildProductPagedResultAsync(
        IQueryable<Domain.Entities.Product> productSource,
        IQueryable<ProductVariant> variantSource,
        ProductListRequest request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page ?? 1, 1);
        var pageSize = Math.Max(request.PageSize ?? 5, 1);
        var searchPattern = string.IsNullOrWhiteSpace(request.Search) ? null : $"%{request.Search.Trim()}%";
        var normalizedStatuses = NormalizeStatuses(request.StatusIds);

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
            Id = p.Id ?? 0,
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
            TotalBooked = p.ProductVariants.SelectMany(v => v.OutputInfos).Where(oi => oi.OutputOrder != null && BookingStatuses.Contains(oi.OutputOrder.StatusId ?? string.Empty)).Sum(oi => (long?)oi.Count) ?? 0
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
                Id = v.Id ?? 0,
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
                HasBeenBooked = v.OutputInfos.Where(oi => oi.OutputOrder != null && BookingStatuses.Contains(oi.OutputOrder.StatusId ?? string.Empty)).Sum(oi => (long?)oi.Count) ?? 0
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var variantLookup = variantRows.GroupBy(row => row.ProductId).ToDictionary(group => group.Key, group => group.ToList());
        var inventoryAlertLevel = await selectRepository.GetInventoryAlertLevelAsync(cancellationToken).ConfigureAwait(false);
        var responses = new List<ProductDetailResponse>(items.Count);
        foreach (var item in items)
        {
            var variants = variantLookup.TryGetValue(item.Id, out var rows) ? rows : [];
            responses.Add(BuildProductDetailResponse(item, variants, inventoryAlertLevel));
        }

        return new PagedResult<ProductDetailResponse>(responses, totalCount, page, pageSize);
    }

    private static ProductDetailResponse BuildProductDetailResponse(Domain.Entities.Product product, long inventoryAlertLevel)
    {
        var variantRows = product.ProductVariants.Select(variant => new VariantRow
        {
            Id = variant.Id ?? 0,
            ProductId = product.Id ?? 0,
            UrlSlug = variant.UrlSlug,
            Price = variant.Price,
            CoverImageUrl = variant.CoverImageUrl,
            Photos = [.. variant.ProductCollectionPhotos.Select(photo => photo.ImageUrl ?? string.Empty)],
            OptionPairs = [.. variant.VariantOptionValues.Select(vov => new OptionPair
                {
                    OptionName = vov.OptionValue?.Option?.Name,
                    OptionValue = vov.OptionValue?.Name
                })],
            Stock = variant.InputInfos.Sum(ii => ii.RemainingCount) ?? 0,
            HasBeenBooked = variant.OutputInfos.Where(oi => oi.OutputOrder != null && BookingStatuses.Contains(oi.OutputOrder.StatusId ?? string.Empty)).Sum(oi => (long?)oi.Count) ?? 0
        }).ToList();

        var summary = new ProductListRow
        {
            Id = product.Id ?? 0,
            Name = product.Name,
            CategoryId = product.CategoryId,
            CategoryName = product.ProductCategory?.Name,
            BrandId = product.BrandId,
            BrandName = product.Brand?.Name,
            Description = product.Description,
            Weight = product.Weight,
            Dimensions = product.Dimensions,
            Wheelbase = product.Wheelbase,
            SeatHeight = product.SeatHeight,
            GroundClearance = product.GroundClearance,
            FuelCapacity = product.FuelCapacity,
            TireSize = product.TireSize,
            FrontSuspension = product.FrontSuspension,
            RearSuspension = product.RearSuspension,
            EngineType = product.EngineType,
            MaxPower = product.MaxPower,
            OilCapacity = product.OilCapacity,
            FuelConsumption = product.FuelConsumption,
            TransmissionType = product.TransmissionType,
            StarterSystem = product.StarterSystem,
            MaxTorque = product.MaxTorque,
            Displacement = product.Displacement,
            BoreStroke = product.BoreStroke,
            CompressionRatio = product.CompressionRatio,
            StatusId = product.StatusId,
            TotalStock = variantRows.Sum(v => v.Stock),
            TotalBooked = variantRows.Sum(v => v.HasBeenBooked)
        };

        return BuildProductDetailResponse(summary, variantRows, inventoryAlertLevel);
    }

    private static ProductDetailResponse BuildProductDetailResponse(ProductListRow summary, List<VariantRow> variants, long inventoryAlertLevel)
    {
        var variantResponses = variants.Select(variant => new ProductVariantDetailResponse
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            UrlSlug = variant.UrlSlug,
            Price = variant.Price,
            CoverImageUrl = variant.CoverImageUrl,
            OptionValues = variant.OptionPairs
                .Where(pair => !string.IsNullOrWhiteSpace(pair.OptionName) && !string.IsNullOrWhiteSpace(pair.OptionValue))
                .ToDictionary(pair => pair.OptionName!, pair => pair.OptionValue!, StringComparer.OrdinalIgnoreCase),
            PhotoCollection = variant.Photos,
            Stock = variant.Stock,
            HasBeenBooked = variant.HasBeenBooked,
            StatusStockId = GetStockStatus(variant.Stock - variant.HasBeenBooked, inventoryAlertLevel)
        })
        .OrderBy(v => v.Stock - v.HasBeenBooked)
        .ThenBy(v => v.UrlSlug)
        .ToList();

        var options = variantResponses
            .SelectMany(variant => variant.OptionValues)
            .GroupBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new ProductOptionDetailResponse
            {
                Name = group.Key,
                Values = [.. group.Select(item => item.Value).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(v => v)]
            })
            .OrderBy(option => option.Name)
            .ToList();

        var availableStock = summary.TotalStock - summary.TotalBooked;
        return new ProductDetailResponse
        {
            Id = summary.Id,
            Name = summary.Name,
            CategoryId = summary.CategoryId,
            CategoryName = summary.CategoryName,
            BrandId = summary.BrandId,
            BrandName = summary.BrandName,
            Description = summary.Description,
            Weight = summary.Weight,
            Dimensions = summary.Dimensions,
            Wheelbase = summary.Wheelbase,
            SeatHeight = summary.SeatHeight,
            GroundClearance = summary.GroundClearance,
            FuelCapacity = summary.FuelCapacity,
            TireSize = summary.TireSize,
            FrontSuspension = summary.FrontSuspension,
            RearSuspension = summary.RearSuspension,
            EngineType = summary.EngineType,
            MaxPower = summary.MaxPower,
            OilCapacity = summary.OilCapacity,
            FuelConsumption = summary.FuelConsumption,
            TransmissionType = summary.TransmissionType,
            StarterSystem = summary.StarterSystem,
            MaxTorque = summary.MaxTorque,
            Displacement = summary.Displacement,
            BoreStroke = summary.BoreStroke,
            CompressionRatio = summary.CompressionRatio,
            StatusId = summary.StatusId,
            CoverImageUrl = variantResponses.FirstOrDefault()?.CoverImageUrl,
            Stock = summary.TotalStock,
            HasBeenBooked = summary.TotalBooked,
            StatusStockId = GetStockStatus(availableStock, inventoryAlertLevel),
            Options = options,
            Variants = variantResponses
        };
    }

    private static List<string> NormalizeStatuses(List<string>? statusIds)
    {
        return statusIds?
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];
    }

    private static string BuildVariantName(IEnumerable<OptionPair> pairs)
    {
        return string.Join(" - ", pairs
            .Where(pair => !string.IsNullOrWhiteSpace(pair.OptionValue))
            .OrderBy(pair => pair.OptionName)
            .Select(pair => pair.OptionValue));
    }

    private static long CalculateAvailableStock(ProductVariant variant)
    {
        var stock = variant.InputInfos.Sum(ii => ii.RemainingCount) ?? 0;
        var booked = variant.OutputInfos.Where(oi => oi.OutputOrder != null && BookingStatuses.Contains(oi.OutputOrder.StatusId ?? string.Empty)).Sum(oi => (long?)oi.Count) ?? 0;
        return stock - booked;
    }

    private static string GetStockStatus(long availableStock, long inventoryAlertLevel)
    {
        if (availableStock <= 0)
        {
            return "out_of_stock";
        }

        return availableStock <= inventoryAlertLevel ? "low_in_stock" : "in_stock";
    }

    private sealed class ProductListRow
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public string? Description { get; set; }
        public decimal? Weight { get; set; }
        public string? Dimensions { get; set; }
        public string? Wheelbase { get; set; }
        public decimal? SeatHeight { get; set; }
        public decimal? GroundClearance { get; set; }
        public decimal? FuelCapacity { get; set; }
        public string? TireSize { get; set; }
        public string? FrontSuspension { get; set; }
        public string? RearSuspension { get; set; }
        public string? EngineType { get; set; }
        public string? MaxPower { get; set; }
        public decimal? OilCapacity { get; set; }
        public string? FuelConsumption { get; set; }
        public string? TransmissionType { get; set; }
        public string? StarterSystem { get; set; }
        public string? MaxTorque { get; set; }
        public decimal? Displacement { get; set; }
        public string? BoreStroke { get; set; }
        public string? CompressionRatio { get; set; }
        public string? StatusId { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public long TotalStock { get; set; }
        public long TotalBooked { get; set; }
    }

    private sealed class VariantRow
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? UrlSlug { get; set; }
        public long? Price { get; set; }
        public string? CoverImageUrl { get; set; }
        public List<string> Photos { get; set; } = [];
        public List<OptionPair> OptionPairs { get; set; } = [];
        public long Stock { get; set; }
        public long HasBeenBooked { get; set; }
    }

    private sealed class OptionPair
    {
        public string? OptionName { get; set; }
        public string? OptionValue { get; set; }
    }
}
