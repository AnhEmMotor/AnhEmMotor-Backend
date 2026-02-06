using Application.ApiContracts.Supplier.Responses;
using Domain.Constants;
using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier;

public interface ISupplierReadRepository
{
    public IQueryable<SupplierEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

    public IQueryable<SupplierWithTotalInputResponse> GetQueryableWithTotalInput(
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<List<SupplierEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<SupplierEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<List<SupplierEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<SupplierWithTotalInputResponse?> GetByIdWithTotalInputAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<bool> IsNameExistsAsync(
        string name,
        int? excludeId = null,
        CancellationToken cancellationToken = default);

    public Task<bool> IsPhoneExistsAsync(
        string phone,
        int? excludeId = null,
        CancellationToken cancellationToken = default);

    public Task<bool> IsTaxIdExistsAsync(
        string taxId,
        int? excludeId = null,
        CancellationToken cancellationToken = default);
}