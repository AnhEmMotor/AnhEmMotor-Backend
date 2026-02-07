using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using OutputEntity = Domain.Entities.Output;

namespace Infrastructure.Repositories.Output;

public class OutputReadRepository(ApplicationDBContext context) : IOutputReadRepository
{
    public IQueryable<OutputEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = context.OutputOrders.IgnoreQueryFilters();

        if(mode == DataFetchMode.ActiveOnly)
        {
            query = query.Where(x => x.DeletedAt == null);
        } else if(mode == DataFetchMode.DeletedOnly)
        {
            query = query.Where(x => x.DeletedAt != null);
        }

        return query
            .Include(x => x.OutputInfos.Where(y => y.DeletedAt == null))
            .ThenInclude(x => x.ProductVariant)
            .ThenInclude(x => x!.Product)
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
            .Include(o => o.OutputInfos)
            .ThenInclude(oi => oi.ProductVariant)
            .ThenInclude(pv => pv!.VariantOptionValues)
            .ThenInclude(vov => vov.OptionValue)
            .ThenInclude(ov => ov!.Option)
            .Include(o => o.OutputStatus)
            .Include(o => o.Buyer)
            .Include(o => o.FinishedByUser)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public async Task<long> GetStockQuantityByVariantIdAsync(int variantId, CancellationToken cancellationToken)
    {
        var validStatusIds = Domain.Constants.Input.InputStatus.FinishInputValues;

        var currentStock = await context.InputInfos
            .AsNoTracking()
            .Where(ii => ii.ProductId == variantId && ii.DeletedAt == null)
            .Join(context.InputReceipts, ii => ii.InputId, i => i.Id, (ii, i) => new { ii, i })
            .Where(x => x.i.DeletedAt == null && validStatusIds.Contains(x.i.StatusId))
            .SumAsync(x => x.ii.RemainingCount ?? 0, cancellationToken)
            .ConfigureAwait(false);

        return currentStock;
    }
}
