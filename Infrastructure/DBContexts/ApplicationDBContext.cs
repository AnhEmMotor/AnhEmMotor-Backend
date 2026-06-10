using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;
using System.Linq.Expressions;
using InputStatus = Domain.Entities.InputStatus;
using ProductStatus = Domain.Entities.ProductStatus;
using SupplierStatus = Domain.Entities.SupplierStatus;

namespace Infrastructure.DBContexts;

public class ApplicationDBContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, Application.Common.Interfaces.IApplicationDbContext
{
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options): base(options)
    {
    }

    protected ApplicationDBContext(DbContextOptions options): base(options)
    {
    }



    public new DbSet<IdentityUserRole<Guid>> UserRoles => Set<IdentityUserRole<Guid>>();

    public virtual DbSet<AnhEmMotor.Domain.Entities.Expense> Expenses { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Input> InputReceipts { get; set; }

    public virtual DbSet<InputInfo> InputInfos { get; set; }

    public virtual DbSet<InputStatus> InputStatuses { get; set; }

    public virtual DbSet<OptionValue> OptionValues { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Output> OutputOrders { get; set; }

    public virtual DbSet<OutputInfo> OutputInfos { get; set; }

    public virtual DbSet<OutputStatus> OutputStatuses { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductCollectionPhoto> ProductCollectionPhotos { get; set; }

    public virtual DbSet<ProductStatus> ProductStatuses { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<ProductVariantColor> ProductVariantColors { get; set; }

    public virtual DbSet<Setting> Settings { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<SupplierContact> SupplierContacts { get; set; }

    public virtual DbSet<Domain.Entities.PartnerType> PartnerTypes { get; set; }

    public virtual DbSet<SupplierStatus> SupplierStatuses { get; set; }

    public virtual DbSet<VariantOptionValue> VariantOptionValues { get; set; }

    public virtual DbSet<ProductCompatibility> ProductCompatibilities { get; set; }

    public virtual DbSet<MediaFile> MediaFiles { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<PredefinedOption> PredefinedOptions { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<TechnologyCategory> TechnologyCategories { get; set; }

    public virtual DbSet<Technology> Technologies { get; set; }

    public virtual DbSet<TechnologyImage> TechnologyImages { get; set; }

    public virtual DbSet<ProductTechnology> ProductTechnologies { get; set; }

    public virtual DbSet<News> News { get; set; }

    public virtual DbSet<NewsCategory> NewsCategories { get; set; }

    public virtual DbSet<Banner> Banners { get; set; }

    public virtual DbSet<BannerAuditLog> BannerAuditLogs { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<ContactReply> ContactReplies { get; set; }


public virtual DbSet<Domain.Entities.SupportRequest> SupportRequests { get; set; }

public virtual DbSet<Domain.Entities.CustomerFeedback> CustomerFeedbacks { get; set; }

public virtual DbSet<Domain.Entities.JobApplication> JobApplications { get; set; }
    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<PlateDossier> PlateDossiers { get; set; }

    public virtual DbSet<RepairOrder> RepairOrders { get; set; }

    public virtual DbSet<RepairOrderDetail> RepairOrderDetails { get; set; }

    public virtual DbSet<ServiceCategory> ServiceCategories { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceEvaluation> ServiceEvaluations { get; set; }

    public virtual DbSet<Lead> Leads { get; set; }

    public virtual DbSet<LeadActivity> LeadActivities { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleDocument> VehicleDocuments { get; set; }

    public virtual DbSet<MaintenanceHistory> MaintenanceHistories { get; set; }

    public virtual DbSet<EmployeeProfile> EmployeeProfiles { get; set; }

    public virtual DbSet<CommissionPolicy> CommissionPolicies { get; set; }

    public virtual DbSet<CommissionRecord> CommissionRecords { get; set; }

    public virtual DbSet<Payroll> Payrolls { get; set; }

    public virtual DbSet<KPI> KPIs { get; set; }

    public virtual DbSet<CommissionPolicyAuditLog> CommissionPolicyAuditLogs { get; set; }

    // Contracts
    public virtual DbSet<ContractTemplate> ContractTemplates { get; set; }
    public virtual DbSet<SalesContract> SalesContracts { get; set; }
    public virtual DbSet<FinanceContract> FinanceContracts { get; set; }
    public virtual DbSet<SupplierContract> SupplierContracts { get; set; }

public virtual DbSet<SupplierContractItem> SupplierContractItems { get; set; }

public virtual DbSet<SupplierContractAuditLog> SupplierContractAuditLogs { get; set; }

    public virtual DbSet<ContractTemplateAuditLog> ContractTemplateAuditLogs { get; set; }

    public virtual DbSet<SupplierFinance> SupplierFinances { get; set; }

    public virtual DbSet<SupplierDebtSettlement> SupplierDebtSettlements { get; set; }

    // Logistics
    public virtual DbSet<Domain.Entities.Logistics.ParcelDeliveryOrder> ParcelDeliveryOrders { get; set; }
    public virtual DbSet<Domain.Entities.Logistics.ParcelDeliveryOrderItem> ParcelDeliveryOrderItems { get; set; }
    public virtual DbSet<Domain.Entities.Logistics.CurrentUnreconciledCod> CurrentUnreconciledCods { get; set; }

    public virtual DbSet<Domain.Entities.Logistics.CarrierPartner> CarrierPartners { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AnhEmMotor.Domain.Entities.Expense>().Property(e => e.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<ContractTemplate>().Property(e => e.Version).HasPrecision(18, 2);
        modelBuilder.Entity<SupplierFinance>().Property(e => e.CurrentDebt).HasPrecision(18, 2);
        modelBuilder.Entity<ContractTemplate>().Property(ct => ct.Version).HasPrecision(18, 2);
        modelBuilder.Entity<SupplierFinance>().Property(sf => sf.CurrentDebt).HasPrecision(18, 2);
        modelBuilder.Entity<Domain.Entities.Logistics.CurrentUnreconciledCod>().Property(e => e.Value).HasPrecision(18, 2);
        modelBuilder.Entity<Domain.Entities.Logistics.ParcelDeliveryOrder>().Property(e => e.CodAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Domain.Entities.Logistics.ParcelDeliveryOrder>().Property(e => e.ShippingCost).HasPrecision(18, 2);
        modelBuilder.Entity<Domain.Entities.Logistics.CarrierPartner>().Property(e => e.MaxParcelWeightKg).HasPrecision(18, 2);

        modelBuilder.Entity<Domain.Entities.Logistics.ParcelDeliveryOrder>()
            .HasMany(p => p.Items)
            .WithOne(i => i.ParcelDeliveryOrder)
            .HasForeignKey(i => i.ParcelDeliveryOrderId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ApplicationUser>().ToTable("Users");
        modelBuilder.Entity<ApplicationRole>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        modelBuilder.Entity<Permission>().ToTable("Permissions");
        modelBuilder.Entity<RolePermission>().ToTable("RolePermissions");
        if (string.Compare(Database.ProviderName, "Npgsql.EntityFrameworkCore.PostgreSQL") != 0)
        {
            modelBuilder.Entity<ProductCategory>().HasAnnotation("Relational:Collation", "utf8mb4_unicode_ci");
        }
        modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PredefinedOption>().HasIndex(p => p.Key).IsUnique();
        modelBuilder.Entity<Option>()
            .HasOne<PredefinedOption>()
            .WithMany()
            .HasPrincipalKey(p => p.Key)
            .HasForeignKey(o => o.Name)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProductCollectionPhoto>()
            .HasOne(p => p.ProductVariant)
            .WithMany(v => v.ProductCollectionPhotos)
            .HasForeignKey(p => p.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<VariantOptionValue>()
            .HasOne(v => v.ProductVariant)
            .WithMany(pv => pv.VariantOptionValues)
            .HasForeignKey(v => v.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ProductVariantColor>()
            .HasOne(pvc => pvc.ProductVariant)
            .WithMany(pv => pv.ProductVariantColors)
            .HasForeignKey(pvc => pvc.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ProductTechnology>().HasKey(pt => new { pt.ProductId, pt.TechnologyId });
        modelBuilder.Entity<ProductTechnology>().HasKey(pt => pt.Id);
        modelBuilder.Entity<ProductStatus>().HasKey(ps => ps.Key);
        modelBuilder.Entity<ProductStatus>()
            .HasData(new ProductStatus { Key = "for-sale" }, new ProductStatus { Key = "out-of-business" });
        modelBuilder.Entity<InputStatus>().HasKey(ins => ins.Key);
        modelBuilder.Entity<Domain.Entities.PartnerType>().HasKey(pt => pt.Key);
        modelBuilder.Entity<Domain.Entities.PartnerType>()
            .HasData(
                new Domain.Entities.PartnerType { Key = Domain.Constants.PartnerType.Supplier },
                new Domain.Entities.PartnerType { Key = Domain.Constants.PartnerType.Financial },
                new Domain.Entities.PartnerType { Key = Domain.Constants.PartnerType.Insurance });
        modelBuilder.Entity<OutputStatus>().HasKey(ous => ous.Key);
        modelBuilder.Entity<ProductTechnology>()
            .HasOne(pt => pt.Product)
            .WithMany(p => p.ProductTechnologies)
            .HasForeignKey(pt => pt.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ProductTechnology>()
            .HasOne(pt => pt.Technology)
            .WithMany(t => t.ProductTechnologies)
            .HasForeignKey(pt => pt.TechnologyId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ProductCompatibility>()
            .HasOne(pc => pc.BaseProduct)
            .WithMany(p => p.CompatibleWith)
            .HasForeignKey(pc => pc.BaseProductId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ProductCompatibility>()
            .HasOne(pc => pc.CompatibleVehicleModel)
            .WithMany(p => p.SupportedBy)
            .HasForeignKey(pc => pc.CompatibleVehicleModelId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<News>()
            .HasOne(n => n.Category)
            .WithMany(c => c.News)
            .HasForeignKey(n => n.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<News>()
            .HasOne(n => n.Author)
            .WithMany()
            .HasForeignKey(n => n.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<MaintenanceHistory>()
            .HasOne(mh => mh.Vehicle)
            .WithMany(v => v.MaintenanceHistories)
            .HasForeignKey(mh => mh.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<VehicleDocument>()
            .HasOne(vd => vd.Vehicle)
            .WithMany(v => v.Documents)
            .HasForeignKey(vd => vd.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.InputInfo)
            .WithMany(ii => ii.Vehicles)
            .HasForeignKey(v => v.InputInfoId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.OutputInfo)
            .WithMany(oi => oi.Vehicles)
            .HasForeignKey(v => v.OutputInfoId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InputInfo>()
            .HasOne(ii => ii.ProductVariantColor)
            .WithMany()
            .HasForeignKey(ii => ii.ProductVariantColorId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<OutputInfo>()
            .HasOne(oi => oi.ProductVariantColor)
            .WithMany()
            .HasForeignKey(oi => oi.ProductVariantColorId)
            .OnDelete(DeleteBehavior.Restrict);
        var isNotSqlServer = string.Compare(Database.ProviderName, "Microsoft.EntityFrameworkCore.SqlServer") != 0;
        var isPostgres = string.Compare(Database.ProviderName, "Npgsql.EntityFrameworkCore.PostgreSQL") == 0;
        if (isNotSqlServer)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (string.Compare(tableName, "Roles") == 0)
                {
                    var index = entityType.GetIndexes()
                        .FirstOrDefault(i => string.Compare(i.GetDatabaseName(), "RoleNameIndex") == 0);
                    index?.SetFilter(null);
                }
                if (string.Compare(tableName, "Users") == 0)
                {
                    var index = entityType.GetIndexes()
                        .FirstOrDefault(i => string.Compare(i.GetDatabaseName(), "UserNameIndex") == 0);
                    index?.SetFilter(null);
                }
                foreach (var property in entityType.GetProperties())
                {
                    var columnType = property.GetColumnType();
                    if (columnType is not null &&
                        (columnType.Contains("nvarchar", StringComparison.OrdinalIgnoreCase) ||
                            columnType.Contains("uniqueidentifier", StringComparison.OrdinalIgnoreCase) ||
                            columnType.Contains("datetimeoffset", StringComparison.OrdinalIgnoreCase) ||
                            columnType.Contains("rowversion", StringComparison.OrdinalIgnoreCase) ||
                            columnType.Contains("bit", StringComparison.OrdinalIgnoreCase)))
                    {
                        property.SetColumnType(null);
                    }
                    if (isPostgres &&
                        (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?)))
                    {
                        property.SetValueConverter(
                            new ValueConverter<DateTimeOffset, DateTimeOffset>(v => v.ToUniversalTime(), v => v));
                    }
                    if (!isPostgres &&
                        (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?)))
                    {
                        property.SetValueConverter(typeof(DateTimeOffsetToBinaryConverter));
                    }
                }
            }
        }
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var deletedAtProperty = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
                var nullConstant = Expression.Constant(null, typeof(DateTimeOffset?));
                var body = Expression.Equal(deletedAtProperty, nullConstant);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(body, parameter));
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedAt == null)
                    {
                        entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                    }
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    public IQueryable<T> All<T>() where T : class => Set<T>().IgnoreQueryFilters();

    public IQueryable<T> DeletedOnly<T>() where T : BaseEntity => All<T>().Where(e => e.DeletedAt != null);

    public void Restore(BaseEntity entity)
    {
        entity.DeletedAt = null;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        Entry(entity).State = EntityState.Modified;
    }

    public void SoftDeleteUsingSetColumn<T>(T entity) where T : BaseEntity
    {
        entity.DeletedAt = DateTimeOffset.UtcNow;
        Entry(entity).State = EntityState.Modified;
    }

    public void RestoreDeleteUsingSetColumn<T>(T entity) where T : BaseEntity
    {
        entity.DeletedAt = null;
        Entry(entity).State = EntityState.Modified;
    }

    public void SoftDeleteUsingSetColumnRange<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        foreach (var entity in entities)
        {
            entity.DeletedAt = DateTimeOffset.UtcNow;
            Entry(entity).State = EntityState.Modified;
        }
    }

    public void RestoreDeleteUsingSetColumnRange<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        foreach (var entity in entities)
        {
            entity.DeletedAt = null;
            Entry(entity).State = EntityState.Modified;
        }
    }

    public IQueryable<T> GetQuery<T>(DataFetchMode mode) where T : BaseEntity
    {
        return mode switch
        {
            DataFetchMode.ActiveOnly => Set<T>(),
            DataFetchMode.DeletedOnly => DeletedOnly<T>(),
            DataFetchMode.All => All<T>(),
            _ => Set<T>()
        };
    }
}
