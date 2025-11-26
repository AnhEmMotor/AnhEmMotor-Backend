using Application.Interfaces.Repositories.Input;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using InputEntity = Domain.Entities.Input;

namespace Infrastructure.Repositories.Input;

public class InputReadRepository(ApplicationDBContext context) : IInputReadRepository
{
    public IQueryable<InputEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<InputEntity>(mode);
    }

    public Task<IEnumerable<InputEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<InputEntity>(mode)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<InputEntity>>(t => t.Result, cancellationToken);
    }

    public Task<InputEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<InputEntity>(mode)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<IEnumerable<InputEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<InputEntity>(mode)
            .Where(i => ids.Contains(i.Id))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<InputEntity>>(t => t.Result, cancellationToken);
    }

    public Task<InputEntity?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<InputEntity>(mode)
            .Include(i => i.InputInfos)
                .ThenInclude(ii => ii.ProductVariant)
                    .ThenInclude(pv => pv!.Product)
            .Include(i => i.InputInfos)
                .ThenInclude(ii => ii.ProductVariant)
                    .ThenInclude(pv => pv!.VariantOptionValues)
                        .ThenInclude(vov => vov.OptionValue)
                            .ThenInclude(ov => ov!.Option)
            .Include(i => i.Supplier)
            .Include(i => i.InputStatus)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public IQueryable<InputEntity> GetBySupplierIdAsync(
        int supplierId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<InputEntity>(mode)
            .Include(i => i.InputInfos)
                .ThenInclude(ii => ii.ProductVariant)
                    .ThenInclude(pv => pv!.Product)
            .Where(i => i.SupplierId == supplierId)
            .OrderByDescending(i => i.CreatedAt);
    }
}
