using Domain.Constants;
using Domain.Primitives;
using System.Linq.Expressions;
using SieveModel = global::Sieve.Models.SieveModel;

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

    public Task<List<Domain.Entities.Vehicle>> GetByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default);

    public Task<List<Domain.Entities.Vehicle>> GetVehiclesForAssignmentAsync(
        IEnumerable<int> productVariantIds,
        CancellationToken cancellationToken = default);

    public Task<bool> ExistsByVinAsync(string vin, CancellationToken cancellationToken = default);

    public Task<bool> ExistsByEngineNumberAsync(string engineNumber, CancellationToken cancellationToken = default);

    public Task<bool> ExistsByVinAsync(
        string vin,
        int productVariantId,
        int? productVariantColorId,
        CancellationToken cancellationToken = default);

    public Task<bool> ExistsByEngineNumberAsync(
        string engineNumber,
        int productVariantId,
        int? productVariantColorId,
        CancellationToken cancellationToken = default);

    public Task<List<Domain.Entities.Vehicle>> GetVehiclesByReceiptInfoIdAsync(
        int receiptInfoId,
        CancellationToken cancellationToken = default);

    public Task<Domain.Entities.Vehicle?> GetByVinAsync(string vin, CancellationToken cancellationToken = default);
}

