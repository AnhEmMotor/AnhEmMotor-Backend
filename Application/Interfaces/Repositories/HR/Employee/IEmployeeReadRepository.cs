using Domain.Entities;

namespace Application.Interfaces.Repositories.HR.Employee;

public interface IEmployeeReadRepository
{
    public Task<List<EmployeeProfile>> GetAllWithUsersAsync(CancellationToken cancellationToken = default);

    public Task<EmployeeProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    public Task<List<KPI>> GetAllWithKPIsAsync(CancellationToken cancellationToken = default);
}
