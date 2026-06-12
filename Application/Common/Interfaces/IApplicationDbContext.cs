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

    // Logistics
    public DbSet<Domain.Entities.Logistics.ParcelDeliveryOrder> ParcelDeliveryOrders { get; }
    public DbSet<Domain.Entities.Logistics.ParcelDeliveryOrderItem> ParcelDeliveryOrderItems { get; }
    public DbSet<Domain.Entities.Logistics.CurrentUnreconciledCod> CurrentUnreconciledCods { get; }
    public DbSet<Domain.Entities.Logistics.CarrierPartner> CarrierPartners { get; }

    public DbSet<TEntity> Set<TEntity>() where TEntity : class;

     public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

