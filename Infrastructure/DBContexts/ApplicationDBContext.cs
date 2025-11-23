using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.DBContexts
{
    public class ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : DbContext(options)
    {
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
        public virtual DbSet<SupplierStatus> SupplierStatuses { get; set; }
        public virtual DbSet<VariantOptionValue> VariantOptionValues { get; set; }
        public virtual DbSet<MediaFile> MediaFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.AddProperty(AuditingProperties.CreatedAt, typeof(DateTimeOffset?));
                entityType.AddProperty(AuditingProperties.UpdatedAt, typeof(DateTimeOffset?));
                entityType.AddProperty(AuditingProperties.DeletedAt, typeof(DateTimeOffset?));

                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Call(
                    typeof(EF),
                    nameof(EF.Property),
                    [typeof(DateTimeOffset?)],
                    parameter,
                    Expression.Constant(AuditingProperties.DeletedAt));

                var body = Expression.Equal(property, Expression.Constant(null, typeof(DateTimeOffset?)));
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(body, parameter));
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                var hasAuditingProperties = entry.Properties.Any(p => string.Compare(p.Metadata.Name, AuditingProperties.CreatedAt) == 0);
                if (!hasAuditingProperties) continue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Property(AuditingProperties.CreatedAt).CurrentValue = DateTimeOffset.UtcNow;
                        break;

                    case EntityState.Modified:
                        entry.Property(AuditingProperties.UpdatedAt).CurrentValue = DateTimeOffset.UtcNow;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Property(AuditingProperties.DeletedAt).CurrentValue = DateTimeOffset.UtcNow;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        public IQueryable<T> All<T>() where T : class
            => Set<T>().IgnoreQueryFilters();

        public IQueryable<T> DeletedOnly<T>() where T : class
            => All<T>().Where(e => EF.Property<DateTimeOffset?>(e, AuditingProperties.DeletedAt) != null);

        public void Restore(object entity)
        {
            var entry = Entry(entity);

            var hasAuditingProperties = entry.Properties.Any(p => string.Compare(p.Metadata.Name, AuditingProperties.CreatedAt) == 0);
            if (!hasAuditingProperties) return;

            entry.Property(AuditingProperties.DeletedAt).CurrentValue = null;
            entry.Property(AuditingProperties.UpdatedAt).CurrentValue = DateTimeOffset.UtcNow;
            entry.State = EntityState.Modified;
        }

        public void SoftDeleteUsingSetColumn<T>(T entity) where T : class
        {
            var entry = Entry(entity);
            var hasDeletedAt = entry.Properties.Any(p => p.Metadata.Name == AuditingProperties.DeletedAt);
            if (!hasDeletedAt)
            {
                throw new InvalidOperationException($"Entity {typeof(T).Name} does not support Soft Delete (missing DeletedAt shadow property).");
            }
            entry.Property(AuditingProperties.DeletedAt).CurrentValue = DateTimeOffset.UtcNow;
            entry.State = EntityState.Modified;
        }

        public void RestoreSoftDeleteUsingSetColumn<T>(T entity) where T : class
        {
            var entry = Entry(entity);
            var hasDeletedAt = entry.Properties.Any(p => p.Metadata.Name == AuditingProperties.DeletedAt);
            if (!hasDeletedAt)
            {
                throw new InvalidOperationException($"Entity {typeof(T).Name} does not support Soft Delete (missing DeletedAt shadow property).");
            }
            entry.Property(AuditingProperties.DeletedAt).CurrentValue = null;
            entry.State = EntityState.Modified;
        }

        public void SoftDeleteUsingSetColumnRange<T>(IEnumerable<T> entities) where T : class
        {
            var entityType = Model.FindEntityType(typeof(T));
            _ = (entityType?.FindProperty(AuditingProperties.DeletedAt)) ?? throw new InvalidOperationException($"Entity {typeof(T).Name} does not support Soft Delete (missing DeletedAt shadow property).");
            foreach (var entity in entities)
            {
                var entry = Entry(entity);
                entry.Property(AuditingProperties.DeletedAt).CurrentValue = DateTimeOffset.UtcNow;
                entry.State = EntityState.Modified;
            }
        }

        public void RestoreSoftDeleteUsingSetColumnRange<T>(IEnumerable<T> entities) where T : class
        {
            var entityType = Model.FindEntityType(typeof(T));
            _ = (entityType?.FindProperty(AuditingProperties.DeletedAt)) ?? throw new InvalidOperationException($"Entity {typeof(T).Name} does not support Soft Delete (missing DeletedAt shadow property).");
            foreach (var entity in entities)
            {
                var entry = Entry(entity);
                entry.Property(AuditingProperties.DeletedAt).CurrentValue = null;
                entry.State = EntityState.Modified;
            }
        }
    } 
}