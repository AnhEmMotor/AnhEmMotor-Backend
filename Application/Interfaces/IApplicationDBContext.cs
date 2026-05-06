using Domain.Entities;
using Domain.Entities.HR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IApplicationDBContext
{
    DbSet<EmployeeProfile> EmployeeProfiles { get; }
    DbSet<CommissionPolicy> CommissionPolicies { get; }
    DbSet<CommissionRecord> CommissionRecords { get; }
    DbSet<Payroll> Payrolls { get; }
    DbSet<KPI> KPIs { get; }
    DbSet<CommissionPolicyAuditLog> CommissionPolicyAuditLogs { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
