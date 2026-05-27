using Application.ApiContracts.Input.Responses;
using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using System.Linq.Expressions;
using InputEntity = Domain.Entities.Input;

namespace Application.Interfaces.Repositories.Input;

public interface IInputReadRepository
{
    public Task<InventoryReceiptStatsResponse> GetStatsAsync(CancellationToken cancellationToken);

    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<InputEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    public Task<List<InputEntity>> GetBySupplierIdAsync(
        int supplierId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<InputEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<InputEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<InputEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<InputEntity?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<InputEntity>> GetBySupplierIdsAsync(
        IEnumerable<int> supplierIds,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);
}
