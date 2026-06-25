using Application.ApiContracts.InventoryReceipt.Responses;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Sieve.Models;
using System.Linq.Expressions;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;
using InventoryReceiptInfoEntity = Domain.Entities.InventoryReceiptInfo;

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

    public Task<InventoryReceiptInfoEntity?> GetInfoByIdAsync(int id, CancellationToken cancellationToken);

    public Task<List<InventoryReceiptInfoEntity>> GetInfosByInventoryReceiptIdsAsync(
        IEnumerable<int> inventoryReceiptIds,
        CancellationToken cancellationToken);

    public Task<List<InventoryReceiptInfoEntity>> GetInfosByVariantAsync(
        int variantId,
        int? colorId,
        CancellationToken cancellationToken);

    public Task<List<InventoryReceiptAuditLog>> GetAuditLogsAsync(
        int inventoryReceiptId,
        CancellationToken cancellationToken);

    public Task<List<InventoryReceiptInfoAuditLog>> GetInfoAuditLogsAsync(
        int inventoryReceiptId,
        CancellationToken cancellationToken);

    public Task<List<VehicleAuditLog>> GetVehicleAuditLogsAsync(
        int inventoryReceiptId,
        CancellationToken cancellationToken);
}

