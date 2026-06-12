using Application.ApiContracts.SalesContracts.Responses;
using Domain.Entities;
using Domain.Primitives;
using Sieve.Models;

namespace Application.Interfaces.Repositories.SalesContract;

public interface ISalesContractReadRepository
{
    Task<PagedResult<SalesContractResponse>> GetPagedAsync(
        SieveModel sieveModel,
        CancellationToken cancellationToken = default);

    Task<global::Domain.Entities.SalesContract?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<global::Domain.Entities.SalesContract>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<global::Domain.Entities.SalesContract?> GetByOrderIdAsync(
        int? orderId,
        CancellationToken cancellationToken = default);

    Task<bool> IsContractNumberExistsAsync(
        string contractNumber,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<int> CountByStatusAsync(string status, CancellationToken cancellationToken = default);

    Task<int> CountOverdueAsync(CancellationToken cancellationToken = default);
}
