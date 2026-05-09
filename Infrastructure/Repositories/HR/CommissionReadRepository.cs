using Application.Interfaces;
using Application.Interfaces.Repositories.HR;
using Domain.Entities.HR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.HR;

public class CommissionReadRepository(IApplicationDBContext context) : ICommissionReadRepository
{
    public Task<List<CommissionRecord>> GetRecordsAsync(CancellationToken cancellationToken = default)
    {
        return context.CommissionRecords
            .Include(r => r.EmployeeProfile)
            .Include(r => r.Output)
            .OrderByDescending(r => r.DateEarned)
            .ToListAsync(cancellationToken);
    }
}
