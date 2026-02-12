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
        return context.GetQuery<SupplierEntity>(mode)
            .GroupJoin(
                context.GetQuery<Domain.Entities.Input>(DataFetchMode.ActiveOnly)
                    .Where(i => i.StatusId == Domain.Constants.Input.InputStatus.Finish),
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
                    Notes = x.supplier.Notes,
                    TaxIdentificationNumber = x.supplier.TaxIdentificationNumber,
                    TotalInput =
                        x.inputs.SelectMany(i => i.InputInfos).Sum(ii => (ii.Count ?? 0) * (ii.InputPrice ?? 0))
                });
    }

    public Task<SupplierWithTotalInputResponse?> GetByIdWithTotalInputAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    { return GetQueryableWithTotalInput(mode).FirstOrDefaultAsync(x => x.Id == id, cancellationToken); }

    public Task<List<SupplierEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    { return context.GetQuery<SupplierEntity>(mode).ToListAsync(cancellationToken); }

    public Task<SupplierEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    { return context.GetQuery<SupplierEntity>(mode).FirstOrDefaultAsync(s => s.Id == id, cancellationToken); }

    public Task<List<SupplierEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    { return context.GetQuery<SupplierEntity>(mode).Where(s => ids.Contains(s.Id)).ToListAsync(cancellationToken); }

    public Task<bool> IsNameExistsAsync(
        string name,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(DataFetchMode.All)
            .AnyAsync(
                x => string.Compare(x.Name, name) == 0 && (!excludeId.HasValue || x.Id != excludeId),
                cancellationToken);
    }

    public Task<bool> IsPhoneExistsAsync(
        string phone,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(DataFetchMode.All)
            .AnyAsync(
                x => string.Compare(x.Phone, phone) == 0 && (!excludeId.HasValue || x.Id != excludeId),
                cancellationToken);
    }

    public Task<bool> IsTaxIdExistsAsync(
        string taxId,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(DataFetchMode.All)
            .AnyAsync(
                x => string.Compare(x.TaxIdentificationNumber, taxId) == 0 && (!excludeId.HasValue || x.Id != excludeId),
                cancellationToken);
    }

    public Task<bool> IsEmailExistsAsync(
        string email,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(DataFetchMode.All)
            .AnyAsync(
                x => string.Compare(x.Email, email) == 0 && (!excludeId.HasValue || x.Id != excludeId),
                cancellationToken);
    }
}