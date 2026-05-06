using Domain.Entities.HR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Interfaces;

public interface IApplicationDBContext
{
    public DbSet<EmployeeProfile> EmployeeProfiles { get; }

    public DbSet<CommissionPolicy> CommissionPolicies { get; }

    public DbSet<CommissionRecord> CommissionRecords { get; }

    public DbSet<Payroll> Payrolls { get; }

    public DbSet<KPI> KPIs { get; }

    public DbSet<CommissionPolicyAuditLog> CommissionPolicyAuditLogs { get; }

    public DatabaseFacade Database { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
