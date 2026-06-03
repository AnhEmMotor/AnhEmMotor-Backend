using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<RepairOrder> RepairOrders { get; }
    public DbSet<RepairOrderDetail> RepairOrderDetails { get; }
    public DbSet<InputInfo> InputInfos { get; }
    public DbSet<Output> OutputOrders { get; }
    public DbSet<EmployeeProfile> EmployeeProfiles { get; }
    public DbSet<AnhEmMotor.Domain.Entities.Expense> Expenses { get; }
    public DbSet<ContractTemplate> ContractTemplates { get; }
    public DbSet<ContractTemplateAuditLog> ContractTemplateAuditLogs { get; }
    public DbSet<TEntity> Set<TEntity>() where TEntity : class;
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
