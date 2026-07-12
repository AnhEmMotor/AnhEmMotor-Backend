using ReturnRequestEntity = Domain.Entities.ReturnRequest;

namespace Application.Interfaces.Repositories.ReturnRequest;

public interface IReturnRequestWriteRepository
{
    public Task AddAsync(ReturnRequestEntity entity, CancellationToken cancellationToken = default);

    public Task UpdateAsync(ReturnRequestEntity entity, CancellationToken cancellationToken = default);
}
