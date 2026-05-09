using Application.Interfaces.Repositories.HR;
using Domain.Entities.HR;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Repositories.HR;

public class EmployeeReadRepository(ApplicationDBContext context) : IEmployeeReadRepository
{
    public Task<List<EmployeeProfile>> GetAllWithUsersAsync(CancellationToken cancellationToken = default)
    {
        return context.EmployeeProfiles
            .Include(e => e.User)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<EmployeeProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return context.EmployeeProfiles
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}
