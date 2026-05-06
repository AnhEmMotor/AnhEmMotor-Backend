using Application.Interfaces.Repositories.HR;
using Domain.Entities.HR;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.HR;

public class EmployeeReadRepository(ApplicationDBContext context) : IEmployeeReadRepository
{
    public async Task<List<EmployeeProfile>> GetAllWithUsersAsync(CancellationToken cancellationToken = default)
    {
        return await context.EmployeeProfiles
            .Include(e => e.User)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.EmployeeProfiles
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}
