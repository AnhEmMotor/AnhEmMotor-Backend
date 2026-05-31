using Application.ApiContracts.InventoryReceipt.Responses;
using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using System.Linq.Expressions;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;

namespace Application.Interfaces.Repositories.InventoryReceipt;

public interface IInventoryReceiptReadRepository
{
    public Task<InventoryReceiptStatsResponse> GetStatsAsync(CancellationToken cancellationToken);

    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<InventoryReceiptEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    public Task<List<InventoryReceiptEntity>> GetBySupplierIdAsync(
        int supplierId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<InventoryReceiptEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<InventoryReceiptEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<InventoryReceiptEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<InventoryReceiptEntity?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<InventoryReceiptEntity>> GetBySupplierIdsAsync(
        IEnumerable<int> supplierIds,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<Domain.Entities.InventoryReceiptInfo?> GetInfoByIdAsync(
        int id,
        CancellationToken cancellationToken);

    public Task<List<Domain.Entities.InventoryReceiptInfo>> GetInfosByVariantAsync(
        int variantId,
        int? colorId,
        CancellationToken cancellationToken);
}
