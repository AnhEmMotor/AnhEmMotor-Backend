using Application.ApiContracts.SalesContracts.Responses;
using Domain.Entities;
using Domain.Primitives;
using Sieve.Models;

namespace Application.Interfaces.Repositories.SalesContract;

public interface ISalesContractReadRepository
{
    public Task<PagedResult<SalesContractResponse>> GetPagedAsync(
        SieveModel sieveModel,
        CancellationToken cancellationToken = default);

    public Task<global::Domain.Entities.SalesContract?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    public Task<List<global::Domain.Entities.SalesContract>> GetAllAsync(
        CancellationToken cancellationToken = default);

    public Task<global::Domain.Entities.SalesContract?> GetByOrderIdAsync(
        int? orderId,
        CancellationToken cancellationToken = default);

    public Task<bool> IsContractNumberExistsAsync(
        string contractNumber,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    public Task<int> CountAsync(CancellationToken cancellationToken = default);

    public Task<int> CountByStatusAsync(string status, CancellationToken cancellationToken = default);

    public Task<int> CountOverdueAsync(CancellationToken cancellationToken = default);
}

