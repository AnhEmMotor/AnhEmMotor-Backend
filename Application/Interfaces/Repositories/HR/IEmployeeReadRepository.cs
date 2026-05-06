using Domain.Entities.HR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.HR;

public interface IEmployeeReadRepository
{
    public Task<List<EmployeeProfile>> GetAllWithUsersAsync(CancellationToken cancellationToken = default);
    public Task<EmployeeProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
