using ReturnRequestEntity = Domain.Entities.ReturnRequest;

namespace Application.Interfaces.Repositories.ReturnRequest;

public interface IReturnRequestWriteRepository
{
    Task AddAsync(ReturnRequestEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ReturnRequestEntity entity, CancellationToken cancellationToken = default);
}
