using Application.Common.Models;
using Domain.Constants;
using Domain.Primitives;
using SieveModel = global::Sieve.Models.SieveModel;
using System.Linq.Expressions;

namespace Application.Interfaces.Repositories.Vehicle;

public interface IVehicleReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<Domain.Entities.Vehicle, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    public Task<List<Domain.Entities.Vehicle>> GetVehiclesAsync(
        string? search,
        CancellationToken cancellationToken = default);

    public Task<Domain.Entities.Vehicle?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    public Task<bool> ExistsByVinAsync(string vin, CancellationToken cancellationToken = default);

    public Task<bool> ExistsByEngineNumberAsync(string engineNumber, CancellationToken cancellationToken = default);
}
