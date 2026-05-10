using Application.Interfaces.Repositories.HR;
using Domain.Entities.HR;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Repositories.HR;

public class CommissionReadRepository(ApplicationDBContext context) : ICommissionReadRepository
{
    public Task<List<CommissionRecord>> GetRecordsAsync(CancellationToken cancellationToken = default)
    {
        return context.CommissionRecords
            .Include(r => r.EmployeeProfile)
            .Include(r => r.Output)
            .OrderByDescending(r => r.DateEarned)
            .ToListAsync(cancellationToken);
    }

    public Task<List<CommissionRecord>> GetRecordsByStatusAsync(
        CommissionStatus status,
        int? employeeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.CommissionRecords.Where(r => r.Status == status);
        if (employeeId.HasValue)
        {
            query = query.Where(r => r.EmployeeProfileId == employeeId.Value);
        }
        return query.ToListAsync(cancellationToken);
    }

    public Task<List<CommissionRecord>> GetRecordsByEmployeeIdAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        return context.CommissionRecords
            .Where(r => r.EmployeeProfileId == employeeId)
            .ToListAsync(cancellationToken);
    }
}
