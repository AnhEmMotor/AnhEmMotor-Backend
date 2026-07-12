using Domain.Primitives;
using Sieve.Models;
using ReturnRequestEntity = Domain.Entities.ReturnRequest;

namespace Application.Interfaces.Repositories.ReturnRequest;

public interface IReturnRequestReadRepository
{
    public Task<PagedResult<ReturnRequestEntity>> GetPagedAsync(
        SieveModel sieveModel,
        CancellationToken cancellationToken = default);

    public Task<ReturnRequestEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
