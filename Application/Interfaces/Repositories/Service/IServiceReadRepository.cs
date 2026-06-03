using Domain.Primitives;
using ServiceEntity = Domain.Entities.Service;

namespace Application.Interfaces.Repositories.Service
{
    public interface IServiceReadRepository
    {
public Task<IEnumerable<ServiceEntity>> GetAllAsync(
            CancellationToken cancellationToken = default);

public Task<ServiceEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

public Task<bool> ExistsAsync(
            int id,
            CancellationToken cancellationToken = default);

public Task<bool> NameExistsAsync(
            string name,
            CancellationToken cancellationToken = default);

public Task<bool> NameExistsExcludingAsync(
            string name,
            int excludeId,
            CancellationToken cancellationToken = default);

        IQueryable<ServiceEntity> GetQueryable();
    }
}
