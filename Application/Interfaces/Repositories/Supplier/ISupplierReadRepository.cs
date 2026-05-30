using Application.ApiContracts.Supplier.Responses;
using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier;

public interface ISupplierReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default);

    public Task<PagedResult<TResponse>> GetPagedWithTotalInventoryReceiptAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default);

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

    public Task<List<SupplierEntity>> GetFilteredListAsync(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default);

    public Task<SupplierWithTotalInventoryReceiptResponse?> GetByIdWithTotalInventoryReceiptAsync(
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

    public Task<bool> IsEmailExistsAsync(
        string email,
        int? excludeId = null,
        CancellationToken cancellationToken = default);

    public Task<SupplierStatisticsResponse> GetStatisticsAsync(CancellationToken cancellationToken = default);
}
