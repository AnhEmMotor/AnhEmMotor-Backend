using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.ApiContracts.Output.Responses;
using Domain.Constants;
using Domain.Constants.InventoryReceipt;
using Domain.Constants.Order;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;
using OutputEntity = Domain.Entities.Output;

namespace Infrastructure.Repositories.Output;

public class OutputReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IOutputReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<OutputEntity, bool>>? filter = null,
        bool withoutContract = false,
        CancellationToken cancellationToken = default)
    {
        var query = GetBaseQueryable(mode);
        if (filter != null)
        {
            query = query.Where(filter);
        }
        if (withoutContract)
        {
            query = query.Where(o => !context.Set<global::Domain.Entities.SalesContract>().Any(c => c.OutputId == o.Id));
        }

        if (typeof(TResponse) == typeof(OutputItemResponse))
        {
            return GetPagedOutputItemsAsync<TResponse>(query, sieveModel, mode, cancellationToken);
        }

        return paginator.ApplyAsync<OutputEntity, TResponse>(
            GetQueryable(mode).Where(x => query.Select(y => y.Id).Contains(x.Id)),
            sieveModel,
            mode,
            cancellationToken);
    }

    private IQueryable<OutputEntity> GetBaseQueryable(DataFetchMode mode)
    {
        var query = context.OutputOrders.IgnoreQueryFilters();
        if (mode == DataFetchMode.ActiveOnly)
        {
            query = query.Where(x => x.DeletedAt == null);
        } else if (mode == DataFetchMode.DeletedOnly)
        {
            query = query.Where(x => x.DeletedAt != null);
        }
        return query;
    }

    private IQueryable<OutputEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetBaseQueryable(mode)
            .AsNoTracking()
            .Include(x => x.OutputInfos.Where(y => y.DeletedAt == null))
            .ThenInclude(x => x.ProductVariant)
            .ThenInclude(x => x!.Product)
            .ThenInclude(p => p!.ProductCategory)
            .Include(x => x.OutputInfos.Where(y => y.DeletedAt == null))
            .ThenInclude(x => x.ProductVariant)
            .ThenInclude(x => x!.ProductCollectionPhotos)
            .Include(x => x.OutputInfos.Where(y => y.DeletedAt == null))
            .ThenInclude(x => x.ProductVariantColor)
            .Include(x => x.Buyer)
            .AsSplitQuery();
    }

    private async Task<PagedResult<TResponse>> GetPagedOutputItemsAsync<TResponse>(
        IQueryable<OutputEntity> query,
        SieveModel sieveModel,
        DataFetchMode mode,
        CancellationToken cancellationToken)
    {
        var page = await paginator.ApplyAsync<OutputEntity, OutputEntity>(
                query,
                sieveModel,
                mode,
                cancellationToken)
            .ConfigureAwait(false);
        var ids = (page.Items ?? []).Select(item => item.Id).ToList();
        if (ids.Count == 0)
        {
            return new PagedResult<TResponse>([], page.TotalCount, page.PageNumber, page.PageSize);
        }

        var projectedItems = await GetBaseQueryable(mode)
            .Where(item => ids.Contains(item.Id))
            .Select(item => new OutputItemResponse
            {
                Id = item.Id,
                BuyerId = item.BuyerId.HasValue ? item.BuyerId.Value.ToString() : null,
                BuyerName = item.Buyer != null ? item.Buyer.FullName : null,
                BuyerEmail = item.Buyer != null ? item.Buyer.Email : null,
                BuyerPhone = item.Buyer != null ? item.Buyer.PhoneNumber : null,
                CustomerName = item.CustomerName,
                CustomerAddress = item.CustomerAddress,
                CustomerPhone = item.CustomerPhone,
                CreatedAt = item.CreatedAt,
                StatusId = item.StatusId,
                PaymentMethod = item.PaymentMethod,
                PaymentStatus = item.PaymentStatus,
                Notes = item.Notes,
                DepositRatio = item.DepositRatio,
                Total =
                    (item.OutputInfos
                        .Where(info => info.DeletedAt == null)
                        .Sum(info => (decimal?)((info.Count ?? 0) * (info.Price ?? 0))) ?? 0) +
                    (item.ShippingFee ?? 0),
                DepositAmount = item.DepositRatio.HasValue && item.DepositRatio != 0
                    ? (item.OutputInfos
                        .Where(info => info.DeletedAt == null)
                        .Sum(info => (decimal?)((info.Count ?? 0) * (info.Price ?? 0))) ?? 0) *
                        (item.DepositRatio.Value / 100m)
                    : null,
                RemainingAmount = item.DepositRatio.HasValue && item.DepositRatio != 0
                    ? ((item.OutputInfos
                        .Where(info => info.DeletedAt == null)
                        .Sum(info => (decimal?)((info.Count ?? 0) * (info.Price ?? 0))) ?? 0) +
                        (item.ShippingFee ?? 0)) -
                        ((item.OutputInfos
                            .Where(info => info.DeletedAt == null)
                            .Sum(info => (decimal?)((info.Count ?? 0) * (info.Price ?? 0))) ?? 0) *
                            (item.DepositRatio.Value / 100m))
                    : null,
                IsInventoryLocked = item.StatusId != null &&
                    item.StatusId != "pending" &&
                    item.StatusId != "waiting_deposit" &&
                    item.StatusId != "waiting_installment",
                ExpectedDeliveryDate = item.CreatedAt.HasValue
                    ? item.CreatedAt.Value.AddDays(3)
                    : null,
                Quantity = item.OutputInfos
                    .Where(info => info.DeletedAt == null)
                    .Sum(info => (int?)info.Count) ?? 0,
                ProductName = item.OutputInfos
                    .Where(info => info.DeletedAt == null)
                    .OrderBy(info => info.Id)
                    .Select(info => info.ProductVariant != null && info.ProductVariant.Product != null
                        ? info.ProductVariant.Product.Name
                        : null)
                    .FirstOrDefault(),
                ProductImage = item.OutputInfos
                    .Where(info => info.DeletedAt == null)
                    .OrderBy(info => info.Id)
                    .Select(info => info.ProductVariantColor != null &&
                        info.ProductVariantColor.CoverImageUrl != null &&
                        info.ProductVariantColor.CoverImageUrl != ""
                            ? info.ProductVariantColor.CoverImageUrl
                            : info.ProductVariant != null && info.ProductVariant.CoverImageUrl != null &&
                                info.ProductVariant.CoverImageUrl != ""
                                ? info.ProductVariant.CoverImageUrl
                                : info.ProductVariant != null
                                    ? info.ProductVariant.ProductCollectionPhotos
                                        .OrderBy(photo => photo.Id)
                                        .Select(photo => photo.ImageUrl)
                                        .FirstOrDefault()
                                    : null)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var itemsById = projectedItems.ToDictionary(item => item.Id);
        var responses = ids
            .Where(itemsById.ContainsKey)
            .Select(id => (TResponse)(object)itemsById[id])
            .ToList();
        return new PagedResult<TResponse>(responses, page.TotalCount, page.PageNumber, page.PageSize);
    }

    public Task<IEnumerable<OutputEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);
        return query
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<OutputEntity>>(t => t.Result, cancellationToken);
    }

    public async Task<IReadOnlyList<OutputEntity>> GetOrderStatisticsDataAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<OutputEntity>(mode)
            .AsNoTracking()
            .Select(o => new OutputEntity
            {
                Id = o.Id,
                CustomerName = o.CustomerName,
                CustomerPhone = o.CustomerPhone,
                StatusId = o.StatusId,
                PaymentStatus = o.PaymentStatus,
                PaymentMethod = o.PaymentMethod,
                PaidAmount = o.PaidAmount,
                ShippingFee = o.ShippingFee,
                CreatedBy = o.CreatedBy,
                LeadId = o.LeadId,
                CreatedAt = o.CreatedAt,
                LastStatusChangedAt = o.LastStatusChangedAt,
                PaymentExpiredAt = o.PaymentExpiredAt,
                OutputInfos = o.OutputInfos.Select(oi => new Domain.Entities.OutputInfo
                {
                    Id = oi.Id,
                    Price = oi.Price,
                    Count = oi.Count
                }).ToList()
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<OutputEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);
        return query
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<IEnumerable<OutputEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);
        return query
            .Where(o => ids.Contains(o.Id))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<OutputEntity>>(t => t.Result, cancellationToken);
    }

    public Task<OutputEntity?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<OutputEntity>(mode)
            .Include(o => o.OutputInfos)
            .ThenInclude(oi => oi.ProductVariant)
            .ThenInclude(pv => pv!.Product)
            .ThenInclude(p => p!.ProductCategory)
            .Include(o => o.OutputInfos)
            .ThenInclude(oi => oi.ProductVariant)
            .ThenInclude(pv => pv!.VariantOptionValues)
            .ThenInclude(vov => vov.OptionValue)
            .ThenInclude(ov => ov!.Option)
            .Include(o => o.OutputInfos)
            .ThenInclude(oi => oi.ProductVariantColor)
            .Include(o => o.OutputInfos)
            .ThenInclude(oi => oi.Vehicles)
            .Include(o => o.OutputInfos)
            .ThenInclude(oi => oi.ProductVariant)
            .ThenInclude(pv => pv!.ProductCollectionPhotos)
            .Include(o => o.OutputStatus)
            .Include(o => o.Buyer)
            .Include(o => o.FinishedByUser)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public async Task<long> GetStockQuantityByVariantIdAsync(
        int variantId,
        int? colorId,
        CancellationToken cancellationToken)
    {
        var validStatusIds = InventoryReceiptStatus.FinishInventoryReceiptValues;
        var currentStock = await context.InventoryReceiptInfos
            .AsNoTracking()
            .Where(
                ii => ii.PurchaseRequestItem != null &&
                    ii.PurchaseRequestItem.ProductVariantId == variantId &&
                    ii.PurchaseRequestItem.ProductVariantColorId == colorId &&
                    ii.DeletedAt == null)
            .Join(context.InventoryReceipts, ii => ii.InventoryReceiptId, i => i.Id, (ii, i) => new { ii, i })
            .Where(x => x.i.DeletedAt == null && validStatusIds.Contains(x.i.StatusId))
            .SumAsync(x => x.ii.RemainingCount ?? 0, cancellationToken)
            .ConfigureAwait(false);
        return currentStock;
    }

    public Task<List<OutputEntity>> GetExpiredOrdersAsync(
        DateTimeOffset expirationThreshold,
        CancellationToken cancellationToken)
    {
        return GetQueryable()
            .AsNoTracking()
            .Where(
                o => (o.StatusId == OrderStatus.Pending ||
                        o.StatusId == OrderStatus.WaitingDeposit ||
                        o.StatusId == OrderStatus.WaitingInstallment) &&
                    !string.IsNullOrEmpty(o.PaymentMethod) &&
                    o.PaymentMethod != PaymentMethod.COD &&
                    (o.PaymentExpiredAt.HasValue
                        ? o.PaymentExpiredAt.Value < DateTimeOffset.UtcNow
                        : o.CreatedAt < expirationThreshold))
            .OrderBy(o => o.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<List<OutputEntity>> GetByLeadIdAsync(int leadId, CancellationToken cancellationToken = default)
    {
        return GetQueryable()
            .Where(o => o.LeadId == leadId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
