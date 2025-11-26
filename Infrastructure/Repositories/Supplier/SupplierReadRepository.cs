using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using SupplierEntity = Domain.Entities.Supplier;

namespace Infrastructure.Repositories.Supplier;

public class SupplierReadRepository(ApplicationDBContext context) : ISupplierReadRepository
{
    public IQueryable<SupplierEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    { return context.GetQuery<SupplierEntity>(mode); }

    public IQueryable<SupplierWithTotalInputDto> GetQueryableWithTotalInput(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<SupplierEntity>(mode)
            .GroupJoin(
                context.GetQuery<Domain.Entities.Input>(DataFetchMode.ActiveOnly)
                    .Where(i => i.StatusId == InputStatus.Finish),
                supplier => supplier.Id,
                input => input.SupplierId,
                (supplier, inputs) => new
                {
                    Supplier = supplier,
                    Inputs = inputs
                })
            .SelectMany(
                x => x.Inputs.DefaultIfEmpty(),
                (x, input) => new
                {
                    x.Supplier,
                    Input = input
                })
            .GroupBy(x => x.Supplier)
            .Select(g => new SupplierWithTotalInputDto
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                Phone = g.Key.Phone,
                Email = g.Key.Email,
                Address = g.Key.Address,
                StatusId = g.Key.StatusId,
                CreatedAt = g.Key.CreatedAt,
                UpdatedAt = g.Key.UpdatedAt,
                DeletedAt = g.Key.DeletedAt,
                TotalInput = g.Where(x => x.Input != null)
                    .Sum(x => x.Input!.InputInfos.Sum(ii => (long)(ii.Count ?? 0) * (ii.InputPrice ?? 0)))
            });
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