using Application.Interfaces.Repositories.HR.Employee;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Repositories.HR.Employee;

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
        return context.EmployeeProfiles.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public Task<List<KPI>> GetAllWithKPIsAsync(CancellationToken cancellationToken = default)
    {
        return context.KPIs
            .Include(k => k.EmployeeProfile)
                .ThenInclude(ep => ep!.User)
            .OrderByDescending(k => k.PeriodEnd)
            .ToListAsync(cancellationToken);
    }
}
