using Domain.Primitives;
using Sieve.Models;
using ReturnRequestEntity = Domain.Entities.ReturnRequest;

namespace Application.Interfaces.Repositories.ReturnRequest;

public interface IReturnRequestReadRepository
{
    Task<PagedResult<ReturnRequestEntity>> GetPagedAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);
    Task<ReturnRequestEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
