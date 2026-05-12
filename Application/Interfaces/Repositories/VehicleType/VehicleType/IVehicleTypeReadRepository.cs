using Domain.Constants;
using Domain.Primitives;
using System.Linq.Expressions;
using SieveModel = Sieve.Models.SieveModel;

namespace Application.Interfaces.Repositories.VehicleType.VehicleType;

public interface IVehicleTypeReadRepository
{
    public Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<bool> ExistsByNameExceptIdAsync(
        string name,
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<List<Domain.Entities.VehicleType>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<Domain.Entities.VehicleType?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<Domain.Entities.VehicleType, bool>>? filter = null,
        CancellationToken cancellationToken = default);
}
