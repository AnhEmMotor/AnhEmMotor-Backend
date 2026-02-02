using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using InputStatus = Domain.Entities.InputStatus;
using ProductStatus = Domain.Entities.ProductStatus;
using SupplierStatus = Domain.Entities.SupplierStatus;

namespace Infrastructure.DBContexts;

public class ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(
    options)
{
    public new DbSet<IdentityUserRole<Guid>> UserRoles => Set<IdentityUserRole<Guid>>();

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

    public virtual DbSet<Setting> Settings { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<SupplierContact> SupplierContacts { get; set; }

    public virtual DbSet<SupplierStatus> SupplierStatuses { get; set; }

    public virtual DbSet<VariantOptionValue> VariantOptionValues { get; set; }

    public virtual DbSet<MediaFile> MediaFiles { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>().ToTable("Users");
        modelBuilder.Entity<ApplicationRole>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        modelBuilder.Entity<Permission>().ToTable("Permissions");
        modelBuilder.Entity<RolePermission>().ToTable("RolePermissions");

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

        // Check if using SQLite and remove SQL Server-specific configurations
        var isSqlite = Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";

        if(isSqlite)
        {
            // Remove SQL Server-specific annotations for SQLite compatibility
            foreach(var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Remove table filters that use SQL Server syntax
                var tableName = entityType.GetTableName();

                if(tableName == "Roles")
                {
                    var index = entityType.GetIndexes()
                        .FirstOrDefault(i => i.GetDatabaseName() == "RoleNameIndex");

                    if(index is not null)
                    {
                        index.SetFilter(null);
                    }
                }

                if(tableName == "Users")
                {
                    var index = entityType.GetIndexes()
                        .FirstOrDefault(i => i.GetDatabaseName() == "UserNameIndex");

                    if(index is not null)
                    {
                        index.SetFilter(null);
                    }
                }

                // Clear SQL Server-specific column types
                foreach(var property in entityType.GetProperties())
                {
                    var columnType = property.GetColumnType();

                    if(columnType is not null &&
                       (columnType.Contains("nvarchar(MAX)", StringComparison.OrdinalIgnoreCase) ||
                        columnType.Contains("uniqueidentifier", StringComparison.OrdinalIgnoreCase) ||
                        columnType.Contains("datetimeoffset", StringComparison.OrdinalIgnoreCase) ||
                        columnType.Contains("rowversion", StringComparison.OrdinalIgnoreCase) ||
                        columnType.Contains("bit", StringComparison.OrdinalIgnoreCase)))
                    {
                        property.SetColumnType(null);
                    }
                }
            }
        }

        foreach(var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if(typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
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

        foreach(var entry in entries)
        {
            switch(entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
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
        foreach(var entity in entities)
        {
            entity.DeletedAt = DateTimeOffset.UtcNow;
            Entry(entity).State = EntityState.Modified;
        }
    }

    public void RestoreDeleteUsingSetColumnRange<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        foreach(var entity in entities)
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