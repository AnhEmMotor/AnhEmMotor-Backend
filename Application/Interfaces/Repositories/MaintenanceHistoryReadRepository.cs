using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using System.Linq.Expressions;
using MaintenanceHistoryEntity = Domain.Entities.MaintenanceHistory;

namespace Application.Interfaces.Repositories;

public interface IMaintenanceHistoryReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<MaintenanceHistoryEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    public Task<IEnumerable<MaintenanceHistoryEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<MaintenanceHistoryEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<MaintenanceHistoryEntity>> GetByVehicleIdAsync(
        int vehicleId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);
}
