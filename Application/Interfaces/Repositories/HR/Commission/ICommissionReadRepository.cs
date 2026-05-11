using Domain.Entities.HR;

using Domain.Entities;

namespace Application.Interfaces.Repositories.HR.Employee;

public interface ICommissionReadRepository
{
    public Task<List<CommissionRecord>> GetRecordsAsync(CancellationToken cancellationToken = default);

    public Task<List<CommissionRecord>> GetRecordsByStatusAsync(
        CommissionStatus status,
        int? employeeId = null,
        CancellationToken cancellationToken = default);
    
    public Task<List<CommissionRecord>> GetRecordsByEmployeeIdAsync(
        int employeeId,
        CancellationToken cancellationToken = default);
}
