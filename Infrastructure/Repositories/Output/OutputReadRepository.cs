using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Domain.Constants.Input;
using Domain.Constants.Order;
using Domain.Primitives;
using Infrastructure.DBContexts;
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
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        if (filter != null)
        {
            query = query.Where(filter);
        }
        return paginator.ApplyAsync<OutputEntity, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    internal IQueryable<OutputEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = context.OutputOrders.IgnoreQueryFilters();
        if (mode == DataFetchMode.ActiveOnly)
        {
            query = query.Where(x => x.DeletedAt == null);
        } else if (mode == DataFetchMode.DeletedOnly)
        {
            query = query.Where(x => x.DeletedAt != null);
        }
        return query
            .Include(x => x.OutputInfos.Where(y => y.DeletedAt == null))
            .ThenInclude(x => x.ProductVariant)
            .ThenInclude(x => x!.Product)
            .ThenInclude(p => p!.ProductCategory)
            .Include(x => x.OutputInfos.Where(y => y.DeletedAt == null))
            .ThenInclude(x => x.ProductVariant)
            .ThenInclude(x => x!.ProductCollectionPhotos)
            .Include(x => x.OutputStatus)
            .Include(x => x.Buyer)
            .Include(x => x.FinishedByUser)
            .AsSplitQuery();
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
        var validStatusIds = InputStatus.FinishInputValues;
        var currentStock = await context.InputInfos
            .AsNoTracking()
            .Where(ii => ii.ProductId == variantId && ii.ProductVariantColorId == colorId && ii.DeletedAt == null)
            .Join(context.InputReceipts, ii => ii.InputId, i => i.Id, (ii, i) => new { ii, i })
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
            .Where(
                o => (o.StatusId == OrderStatus.Pending || o.StatusId == OrderStatus.WaitingDeposit) &&
                    !string.IsNullOrEmpty(o.PaymentMethod) &&
                    o.PaymentMethod != PaymentMethod.COD &&
                    (o.PaymentExpiredAt.HasValue
                        ? o.PaymentExpiredAt.Value < DateTimeOffset.UtcNow
                        : o.CreatedAt < expirationThreshold))
            .ToListAsync(cancellationToken);
    }
}
