using Domain.Entities.HR;

namespace Application.Interfaces.Repositories.HR;

public interface ICommissionReadRepository
{
    public Task<List<CommissionRecord>> GetRecordsAsync(CancellationToken cancellationToken = default);
}
