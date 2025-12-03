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
        var query = context.InputReceipts.IgnoreQueryFilters();

        if(mode == DataFetchMode.ActiveOnly)
        {
            query = query.Where(x => x.DeletedAt == null);
        } else if(mode == DataFetchMode.DeletedOnly)
        {
            query = query.Where(x => x.DeletedAt != null);
        }

        return query
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
            .ThenInclude(x => x.ProductVariant)
            .ThenInclude(x => x!.Product)
            .Include(x => x.Supplier)
            .Include(x => x.InputStatus);
    }

    public Task<IEnumerable<InputEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);

        return query
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<InputEntity>>(t => t.Result, cancellationToken);
    }

    public Task<InputEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);

        return query
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<IEnumerable<InputEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);

        return query
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<InputEntity>>(t => t.Result, cancellationToken);
    }

    public Task<InputEntity?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);

        return query
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
            .ThenInclude(x => x.ProductVariant)
            .ThenInclude(x => x!.Product)
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
            .ThenInclude(x => x.ProductVariant)
            .ThenInclude(x => x!.VariantOptionValues)
            .ThenInclude(x => x.OptionValue)
            .ThenInclude(x => x!.Option)
            .Include(x => x.Supplier)
            .Include(x => x.InputStatus)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public IQueryable<InputEntity> GetBySupplierIdAsync(
        int supplierId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);

        return query
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
            .ThenInclude(x => x.ProductVariant)
            .ThenInclude(x => x!.Product)
            .Where(x => x.SupplierId == supplierId)
            .OrderByDescending(x => x.CreatedAt);
    }
}