using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using OutputEntity = Domain.Entities.Output;

namespace Infrastructure.Repositories.Output;

public class OutputReadRepository(ApplicationDBContext context) : IOutputReadRepository
{
    public IQueryable<OutputEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<OutputEntity>(mode);
    }

    public Task<IEnumerable<OutputEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<OutputEntity>(mode)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<OutputEntity>>(t => t.Result, cancellationToken);
    }

    public Task<OutputEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<OutputEntity>(mode)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<IEnumerable<OutputEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<OutputEntity>(mode)
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
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public async Task<long> GetStockQuantityByVariantIdAsync(
        int variantId,
        CancellationToken cancellationToken)
    {
        var totalInput = await context.InputInfos
            .Where(ii => ii.ProductId == variantId)
            .Join(
                context.InputReceipts,
                ii => ii.InputId,
                i => i.Id,
                (ii, i) => new { ii, i })
            .Where(x => x.i.StatusId == InputStatus.Finish)
            .SumAsync(x => (long?)x.ii.Count ?? 0, cancellationToken)
            .ConfigureAwait(false);

        var totalOutput = await context.OutputInfos
            .Where(oi => oi.ProductId == variantId)
            .Join(
                context.OutputOrders,
                oi => oi.OutputId,
                o => o.Id,
                (oi, o) => new { oi, o })
            .Where(x => x.o.StatusId != OrderStatus.Cancelled)
            .SumAsync(x => (long?)x.oi.Count ?? 0, cancellationToken)
            .ConfigureAwait(false);

        return totalInput - totalOutput;
    }
}
