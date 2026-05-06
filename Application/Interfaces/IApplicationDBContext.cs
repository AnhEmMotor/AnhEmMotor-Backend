using Domain.Entities.HR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

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
