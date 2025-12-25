using Application.ApiContracts.Supplier.Responses;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using SupplierEntity = Domain.Entities.Supplier;

namespace Infrastructure.Repositories.Supplier;

public class SupplierReadRepository(ApplicationDBContext context) : ISupplierReadRepository
{
    public IQueryable<SupplierEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    { return context.GetQuery<SupplierEntity>(mode); }

    public IQueryable<SupplierWithTotalInputResponse> GetQueryableWithTotalInput(
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = context.GetQuery<SupplierEntity>(mode)
            .GroupJoin(
                context.GetQuery<Domain.Entities.Input>(DataFetchMode.ActiveOnly)
                    .Where(i => i.StatusId == InputStatus.Finish),
                supplier => supplier.Id,
                input => input.SupplierId,
                (supplier, inputs) => new { supplier, inputs })
            .Select(
                x => new SupplierWithTotalInputResponse
                {
                    Id = x.supplier.Id,
                    Name = x.supplier.Name!,
                    Phone = x.supplier.Phone,
                    Email = x.supplier.Email,
                    Address = x.supplier.Address,
                    StatusId = x.supplier.StatusId!,
                    CreatedAt = x.supplier.CreatedAt,
                    UpdatedAt = x.supplier.UpdatedAt,
                    DeletedAt = x.supplier.DeletedAt,
                    TotalInput =
                        x.inputs.SelectMany(i => i.InputInfos).Sum(ii => (ii.Count ?? 0) * (ii.InputPrice ?? 0))
                });
        return query;
    }

    public Task<IEnumerable<SupplierEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<SupplierEntity>(mode)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<SupplierEntity>>(t => t.Result, cancellationToken);
    }

    public Task<SupplierEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<SupplierEntity>(mode)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<IEnumerable<SupplierEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<SupplierEntity>(mode)
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<SupplierEntity>>(t => t.Result, cancellationToken);
    }
}