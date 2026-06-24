using Domain.Constants;
using Domain.Entities;
using Domain.Entities.Logistics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;
using System.Linq.Expressions;
using InventoryReceiptStatus = Domain.Entities.InventoryReceiptStatus;
using ProductStatus = Domain.Entities.ProductStatus;
using SupplierStatus = Domain.Entities.SupplierStatus;

namespace Infrastructure.DBContexts;

public class ApplicationDBContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly IServiceProvider? _serviceProvider;

    public ApplicationDBContext(
        DbContextOptions<ApplicationDBContext> options,
        IServiceProvider? serviceProvider = null): base(options)
    {
        _serviceProvider = serviceProvider;
    }

    protected ApplicationDBContext(DbContextOptions options, IServiceProvider? serviceProvider = null): base(options)
    {
        _serviceProvider = serviceProvider;
    }

    public new DbSet<IdentityUserRole<Guid>> UserRoles => Set<IdentityUserRole<Guid>>();

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<InventoryReceipt> InventoryReceipts { get; set; }

    public virtual DbSet<InventoryReceiptInfo> InventoryReceiptInfos { get; set; }

    public virtual DbSet<InventoryReceiptStatus> InventoryReceiptStatuses { get; set; }

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

    public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public virtual DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    public virtual DbSet<CustomerContact> CustomerContacts { get; set; }

    public virtual DbSet<CustomerContactReply> CustomerContactReplies { get; set; }

    public virtual DbSet<BookingAppointment> BookingAppointments { get; set; }

    public virtual DbSet<NewsArticle> NewsArticles { get; set; }

    public virtual DbSet<PromotionBanner> PromotionBanners { get; set; }

    public virtual DbSet<ServiceBooking> ServiceBookings { get; set; }

    public virtual DbSet<SupportTicket> SupportTickets { get; set; }

    public virtual DbSet<OrderLogistics> OrderLogistics { get; set; }

    public virtual DbSet<TechnologyCategory> TechnologyCategories { get; set; }

    public virtual DbSet<Technology> Technologies { get; set; }

    public virtual DbSet<TechnologyImage> TechnologyImages { get; set; }

    public virtual DbSet<ProductTechnology> ProductTechnologies { get; set; }

    public virtual DbSet<News> News { get; set; }

    public virtual DbSet<NewsProduct> NewsProducts { get; set; }

    public virtual DbSet<NewsCategory> NewsCategories { get; set; }

    public virtual DbSet<NewsComment> NewsComments { get; set; }

    public virtual DbSet<Banner> Banners { get; set; }

    public virtual DbSet<BannerAuditLog> BannerAuditLogs { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<ContactReply> ContactReplies { get; set; }

    public virtual DbSet<SupportRequest> SupportRequests { get; set; }

    public virtual DbSet<CustomerFeedback> CustomerFeedbacks { get; set; }

    public virtual DbSet<JobApplication> JobApplications { get; set; }

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

    public virtual DbSet<ProductQuotation> ProductQuotations { get; set; }

    public virtual DbSet<PurchaseRequest> PurchaseRequests { get; set; }

    public virtual DbSet<PurchaseRequestItem> PurchaseRequestItems { get; set; }

    public virtual DbSet<InventoryLedger> InventoryLedgers { get; set; }

    public virtual DbSet<SupplierDebt> SupplierDebts { get; set; }

    public virtual DbSet<InventoryOnHand> InventoryOnHands { get; set; }

    public virtual DbSet<ContractTemplate> ContractTemplates { get; set; }

    public virtual DbSet<SalesContract> SalesContracts { get; set; }

    public virtual DbSet<FinanceContract> FinanceContracts { get; set; }

    public virtual DbSet<SupplierContract> SupplierContracts { get; set; }

    public virtual DbSet<SupplierContractItem> SupplierContractItems { get; set; }

    public virtual DbSet<SupplierContractAuditLog> SupplierContractAuditLogs { get; set; }

    public virtual DbSet<ContractTemplateAuditLog> ContractTemplateAuditLogs { get; set; }

    public virtual DbSet<SupplierFinance> SupplierFinances { get; set; }

    public virtual DbSet<SupplierDebtLog> SupplierDebtLogs { get; set; }

    public virtual DbSet<InventoryReceiptAuditLog> InventoryReceiptAuditLogs { get; set; }

    public virtual DbSet<InventoryReceiptInfoAuditLog> InventoryReceiptInfoAuditLogs { get; set; }

    public virtual DbSet<VehicleAuditLog> VehicleAuditLogs { get; set; }

    public virtual DbSet<PurchaseRequestAuditLog> PurchaseRequestAuditLogs { get; set; }

    public virtual DbSet<PurchaseRequestItemAuditLog> PurchaseRequestItemAuditLogs { get; set; }

    public virtual DbSet<ParcelDeliveryOrder> ParcelDeliveryOrders { get; set; }

    public virtual DbSet<ParcelDeliveryOrderItem> ParcelDeliveryOrderItems { get; set; }

    public virtual DbSet<CurrentUnreconciledCod> CurrentUnreconciledCods { get; set; }

    public virtual DbSet<CarrierPartner> CarrierPartners { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Expense>().Property(e => e.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<ContractTemplate>().Property(e => e.Version).HasPrecision(18, 2);
        modelBuilder.Entity<SupplierFinance>().Property(e => e.CurrentDebt).HasPrecision(18, 2);
        modelBuilder.Entity<ContractTemplate>().Property(ct => ct.Version).HasPrecision(18, 2);
        modelBuilder.Entity<SupplierFinance>().Property(sf => sf.CurrentDebt).HasPrecision(18, 2);
        modelBuilder.Entity<CurrentUnreconciledCod>().Property(e => e.Value).HasPrecision(18, 2);
        modelBuilder.Entity<ParcelDeliveryOrder>().Property(e => e.CodAmount).HasPrecision(18, 2);
        modelBuilder.Entity<ParcelDeliveryOrder>().Property(e => e.ShippingCost).HasPrecision(18, 2);
        modelBuilder.Entity<CarrierPartner>().Property(e => e.MaxParcelWeightKg).HasPrecision(18, 2);
        modelBuilder.Entity<SupplierDebtLog>().Property(l => l.AmountPaid).HasPrecision(18, 2);
        modelBuilder.Entity<SupplierDebtLog>().Property(l => l.RemainingDebt).HasPrecision(18, 2);
        modelBuilder.Entity<ParcelDeliveryOrder>()
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
        modelBuilder.Entity<InventoryTransaction>()
            .HasOne(i => i.ProductVariant)
            .WithMany(v => v.InventoryTransactions)
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryTransaction>()
            .HasOne(i => i.PerformedByUser)
            .WithMany(u => u.InventoryTransactions)
            .HasForeignKey(i => i.PerformedBy)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<InventoryTransaction>().HasIndex(i => new { i.ProductVariantId, i.PerformedAt });
        modelBuilder.Entity<OrderStatusHistory>()
            .HasOne(h => h.Output)
            .WithMany(o => o.StatusHistories)
            .HasForeignKey(h => h.OutputId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<OrderStatusHistory>()
            .HasOne(h => h.ChangedByUser)
            .WithMany(u => u.OrderStatusHistories)
            .HasForeignKey(h => h.ChangedBy)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<OrderStatusHistory>().HasIndex(h => new { h.OutputId, h.ChangedAt });
        modelBuilder.Entity<CustomerContact>()
            .HasOne(c => c.ProcessedByUser)
            .WithMany(u => u.ProcessedContacts)
            .HasForeignKey(c => c.ProcessedBy)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<CustomerContact>().HasIndex(c => new { c.Status, c.CreatedAt });
        modelBuilder.Entity<CustomerContactReply>()
            .HasOne(r => r.CustomerContact)
            .WithMany(c => c.Replies)
            .HasForeignKey(r => r.ContactId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CustomerContactReply>()
            .HasOne(r => r.RepliedByUser)
            .WithMany(u => u.ContactReplies)
            .HasForeignKey(r => r.RepliedBy)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<CustomerContactReply>().HasIndex(r => new { r.ContactId, r.SentAt });
        modelBuilder.Entity<BookingAppointment>()
            .HasOne(b => b.ProductVariant)
            .WithMany(v => v.BookingAppointments)
            .HasForeignKey(b => b.ProductVariantId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<BookingAppointment>()
            .HasOne(b => b.ConfirmedByUser)
            .WithMany(u => u.ConfirmedBookings)
            .HasForeignKey(b => b.ConfirmedBy)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<BookingAppointment>().HasIndex(b => new { b.Status, b.AppointmentAt });
        modelBuilder.Entity<NewsArticle>()
            .HasOne(n => n.Author)
            .WithMany(u => u.AuthoredNewsArticles)
            .HasForeignKey(n => n.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<NewsArticle>()
            .HasOne(n => n.PublishedByUser)
            .WithMany(u => u.PublishedNewsArticles)
            .HasForeignKey(n => n.PublishedBy)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<NewsArticle>().HasIndex(n => n.Slug).IsUnique();
        modelBuilder.Entity<NewsArticle>().HasIndex(n => new { n.Status, n.PublishedAt });
        modelBuilder.Entity<PromotionBanner>()
            .HasOne(b => b.CreatedByUser)
            .WithMany(u => u.CreatedBanners)
            .HasForeignKey(b => b.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<PromotionBanner>()
            .HasOne(b => b.UpdatedByUser)
            .WithMany(u => u.UpdatedBanners)
            .HasForeignKey(b => b.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<PromotionBanner>().HasIndex(b => new { b.IsEnabled, b.StartDate, b.EndDate });
        modelBuilder.Entity<Vehicle>(
            entity =>
            {
                entity.HasOne(v => v.User).WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(v => v.Product)
                    .WithMany()
                    .HasForeignKey(v => v.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        modelBuilder.Entity<ServiceBooking>(
            entity =>
            {
                entity.HasOne(b => b.Vehicle)
                    .WithMany()
                    .HasForeignKey(b => b.VehicleId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(b => b.AssignedSale)
                    .WithMany()
                    .HasForeignKey(b => b.AssignedSaleId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        modelBuilder.Entity<SupportTicket>(
            entity =>
            {
                entity.HasOne(t => t.Customer)
                    .WithMany()
                    .HasForeignKey(t => t.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.AssignedAdmin)
                    .WithMany()
                    .HasForeignKey(t => t.AssignedAdminId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
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
        modelBuilder.Entity<InventoryReceiptStatus>().HasKey(ins => ins.Key);
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
        modelBuilder.Entity<NewsComment>()
            .HasOne(nc => nc.News)
            .WithMany(n => n.Comments)
            .HasForeignKey(nc => nc.NewsId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<NewsComment>()
            .HasOne(nc => nc.User)
            .WithMany()
            .HasForeignKey(nc => nc.UserId)
            .OnDelete(DeleteBehavior.SetNull);
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
            .HasOne(v => v.InventoryReceiptInfo)
            .WithMany(ii => ii.Vehicles)
            .HasForeignKey(v => v.InventoryReceiptInfoId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.OutputInfo)
            .WithMany(oi => oi.Vehicles)
            .HasForeignKey(v => v.OutputInfoId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.ProductVariant)
            .WithMany()
            .HasForeignKey(v => v.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.ProductVariantColor)
            .WithMany()
            .HasForeignKey(v => v.ProductVariantColorId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryReceiptInfo>()
            .HasOne(ii => ii.PurchaseRequestItem)
            .WithMany(pri => pri.InventoryReceiptInfos)
            .HasForeignKey(ii => ii.PurchaseRequestItemId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<OutputInfo>()
            .HasOne(oi => oi.ProductVariantColor)
            .WithMany()
            .HasForeignKey(oi => oi.ProductVariantColorId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProductQuotation>()
            .HasOne(oi => oi.ProductVariant)
            .WithMany()
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProductQuotation>()
            .HasOne(oi => oi.ProductVariantColor)
            .WithMany()
            .HasForeignKey(oi => oi.ProductVariantColorId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<PurchaseRequestItem>()
            .HasOne(oi => oi.PurchaseRequest)
            .WithMany(q => q.PurchaseRequestItems)
            .HasForeignKey(oi => oi.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
        modelBuilder.Entity<PurchaseRequestItem>()
            .HasOne(oi => oi.ProductVariant)
            .WithMany()
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        modelBuilder.Entity<PurchaseRequestItem>()
            .HasOne(oi => oi.ProductVariantColor)
            .WithMany()
            .HasForeignKey(oi => oi.ProductVariantColorId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryReceipt>()
            .HasOne(oi => oi.PurchaseRequest)
            .WithMany()
            .HasForeignKey(oi => oi.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<SupplierDebt>()
            .HasOne(sd => sd.InventoryReceipt)
            .WithMany(ir => ir.SupplierDebts)
            .HasForeignKey(sd => sd.InventoryReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SupplierDebt>()
            .HasOne(sd => sd.Supplier)
            .WithMany(s => s.SupplierDebts)
            .HasForeignKey(sd => sd.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryReceiptInfo>()
            .HasOne(ii => ii.InventoryReceipt)
            .WithMany(ir => ir.InventoryReceiptInfos)
            .HasForeignKey(ii => ii.InventoryReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<InventoryLedger>()
            .HasOne(il => il.ProductVariant)
            .WithMany()
            .HasForeignKey(il => il.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryLedger>()
            .HasOne(il => il.ProductVariantColor)
            .WithMany()
            .HasForeignKey(il => il.ProductVariantColorId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryOnHand>()
            .HasOne(i => i.ProductVariant)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryOnHand>()
            .HasOne(i => i.ProductVariantColor)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantColorId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<InventoryOnHand>()
            .HasIndex(i => new { i.ProductVariantId, i.ProductVariantColorId, i.Month, i.Year })
            .IsUnique();
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

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
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
        var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return result;
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
