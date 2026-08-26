using Domain.Primitives;
using Sieve.Models;
using ReturnRequestEntity = Domain.Entities.ReturnRequest;

namespace Application.Interfaces.Repositories.ReturnRequest;

public interface IReturnRequestReadRepository
{
    public Task<int> CountAsync(CancellationToken cancellationToken = default);

    public Task<PagedResult<ReturnRequestEntity>> GetPagedAsync(
        SieveModel sieveModel,
        CancellationToken cancellationToken = default);

    public Task<ReturnRequestEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    public Task<bool> HasActiveReturnRequestAsync(int orderId, CancellationToken cancellationToken = default);

    public Task<List<ReturnRequestEntity>> GetCompletedRestockAwaitingArrivalAsync(
        int orderId,
        CancellationToken cancellationToken = default);

    public Task<List<ReturnRequestEntity>> GetByOrderIdAsync(
        int orderId,
        CancellationToken cancellationToken = default);
}
