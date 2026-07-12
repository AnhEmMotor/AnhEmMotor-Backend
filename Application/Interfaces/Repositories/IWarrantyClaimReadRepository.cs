using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using System.Linq.Expressions;
using WarrantyClaimEntity = Domain.Entities.WarrantyClaim;

namespace Application.Interfaces.Repositories;

public interface IWarrantyClaimReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<WarrantyClaimEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    public Task<IEnumerable<WarrantyClaimEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<WarrantyClaimEntity>> GetAllWithDetailsAsync(
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default);

    public Task<WarrantyClaimEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<WarrantyClaimEntity?> GetDetailByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<WarrantyClaimEntity>> GetByVehicleIdAsync(
        int vehicleId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<WarrantyClaimEntity>> GetHistoryByVehicleIdAsync(
        int vehicleId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);
}
