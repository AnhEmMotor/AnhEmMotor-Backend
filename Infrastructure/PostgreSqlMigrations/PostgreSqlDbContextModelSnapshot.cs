using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
{
    [DbContext(typeof(PostgreSqlDbContext))]
    partial class PostgreSqlDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            #pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);
            modelBuilder.Entity(
                "Domain.Entities.ApplicationRole",
                b =>
                {
                    b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
                    b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("text");
                    b.Property<string>("Description").HasColumnType("text");
                    b.Property<string>("Name").HasMaxLength(256).HasColumnType("character varying(256)");
                    b.Property<string>("NormalizedName").HasMaxLength(256).HasColumnType("character varying(256)");
                    b.HasKey("Id");
                    b.HasIndex("NormalizedName").IsUnique().HasDatabaseName("RoleNameIndex");
                    b.ToTable("Roles", (string)null);
                });
            modelBuilder.Entity(
                "Domain.Entities.ApplicationUser",
                b =>
                {
                    b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
                    b.Property<int>("AccessFailedCount").HasColumnType("integer");
                    b.Property<string>("AvatarUrl").HasColumnType("text");
                    b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("text");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTime?>("DateOfBirth").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Email").HasMaxLength(256).HasColumnType("character varying(256)");
                    b.Property<bool>("EmailConfirmed").HasColumnType("boolean");
                    b.Property<string>("FullName").IsRequired().HasColumnType("text");
                    b.Property<string>("Gender").IsRequired().HasColumnType("text");
                    b.Property<bool>("LockoutEnabled").HasColumnType("boolean");
                    b.Property<DateTimeOffset?>("LockoutEnd").HasColumnType("timestamp with time zone");
                    b.Property<string>("NormalizedEmail").HasMaxLength(256).HasColumnType("character varying(256)");
                    b.Property<string>("NormalizedUserName").HasMaxLength(256).HasColumnType("character varying(256)");
                    b.Property<string>("PasswordHash").HasColumnType("text");
                    b.Property<string>("PhoneNumber").HasColumnType("text");
                    b.Property<bool>("PhoneNumberConfirmed").HasColumnType("boolean");
                    b.Property<string>("RefreshToken").HasColumnType("text");
                    b.Property<DateTimeOffset>("RefreshTokenExpiryTime").HasColumnType("timestamp with time zone");
                    b.Property<string>("SecurityStamp").HasColumnType("text");
                    b.Property<string>("Status").IsRequired().HasColumnType("text");
                    b.Property<bool>("TwoFactorEnabled").HasColumnType("boolean");
                    b.Property<string>("UserName").HasMaxLength(256).HasColumnType("character varying(256)");
                    b.HasKey("Id");
                    b.HasIndex("NormalizedEmail").HasDatabaseName("EmailIndex");
                    b.HasIndex("NormalizedUserName").IsUnique().HasDatabaseName("UserNameIndex");
                    b.ToTable("Users", (string)null);
                });
            modelBuilder.Entity(
                "Domain.Entities.Banner",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<int>("ClickCount").HasColumnType("integer").HasColumnName("ClickCount");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("CtaText").HasColumnType("text").HasColumnName("CtaText");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<int>("DisplayOrder").HasColumnType("integer").HasColumnName("DisplayOrder");
                    b.Property<DateTimeOffset?>("EndDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("EndDate");
                    b.Property<string>("ImageUrl").IsRequired().HasColumnType("text").HasColumnName("ImageUrl");
                    b.Property<bool>("IsActive").HasColumnType("boolean").HasColumnName("IsActive");
                    b.Property<string>("LinkUrl").HasColumnType("text").HasColumnName("LinkUrl");
                    b.Property<string>("Placement").HasColumnType("text").HasColumnName("Placement");
                    b.Property<string>("Position").HasColumnType("text").HasColumnName("Position");
                    b.Property<int>("Priority").HasColumnType("integer").HasColumnName("Priority");
                    b.Property<DateTimeOffset?>("StartDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("StartDate");
                    b.Property<string>("Title").IsRequired().HasColumnType("text").HasColumnName("Title");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.Property<int>("ViewCount").HasColumnType("integer").HasColumnName("ViewCount");
                    b.HasKey("Id");
                    b.ToTable("Banner");
                });
            modelBuilder.Entity(
                "Domain.Entities.BannerAuditLog",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("Action").IsRequired().HasColumnType("text").HasColumnName("Action");
                    b.Property<int>("BannerId").HasColumnType("integer").HasColumnName("BannerId");
                    b.Property<string>("ChangedBy").IsRequired().HasColumnType("text").HasColumnName("ChangedBy");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Details").HasColumnType("text").HasColumnName("Details");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("BannerId");
                    b.ToTable("BannerAuditLog");
                });
            modelBuilder.Entity(
                "Domain.Entities.Booking",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("BookingType").IsRequired().HasColumnType("text").HasColumnName("BookingType");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Email").IsRequired().HasColumnType("text").HasColumnName("Email");
                    b.Property<string>("FullName").IsRequired().HasColumnType("text").HasColumnName("FullName");
                    b.Property<string>("Location").IsRequired().HasColumnType("text").HasColumnName("Location");
                    b.Property<string>("Note").IsRequired().HasColumnType("text").HasColumnName("Note");
                    b.Property<string>("PhoneNumber").IsRequired().HasColumnType("text").HasColumnName("PhoneNumber");
                    b.Property<DateTimeOffset>("PreferredDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("PreferredDate");
                    b.Property<int?>("ProductVariantId").HasColumnType("integer").HasColumnName("ProductVariantId");
                    b.Property<string>("Status").IsRequired().HasColumnType("text").HasColumnName("Status");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("ProductVariantId");
                    b.ToTable("Booking");
                });
            modelBuilder.Entity(
                "Domain.Entities.Brand",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Description").HasColumnType("text").HasColumnName("Description");
                    b.Property<string>("LogoUrl").HasColumnType("text").HasColumnName("LogoUrl");
                    b.Property<string>("Name").HasColumnType("text").HasColumnName("Name");
                    b.Property<string>("Origin").HasColumnType("text").HasColumnName("Origin");
                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("bytea")
                        .HasColumnName("RowVersion");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.ToTable("Brand");
                });
            modelBuilder.Entity(
                "Domain.Entities.CommissionPolicy",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<int?>("CategoryId").HasColumnType("integer");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset>("EffectiveDate").HasColumnType("timestamp with time zone");
                    b.Property<Guid?>("EmployeeId").HasColumnType("uuid");
                    b.Property<bool>("IsActive").HasColumnType("boolean");
                    b.Property<string>("Name").IsRequired().HasColumnType("text");
                    b.Property<string>("Notes").HasColumnType("text");
                    b.Property<int?>("ProductId").HasColumnType("integer");
                    b.Property<string>("TargetGroup").HasColumnType("text");
                    b.Property<string>("Type").IsRequired().HasColumnType("text");
                    b.Property<string>("Unit").HasColumnType("text");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.Property<decimal>("Value").HasColumnType("decimal(18,2)");
                    b.HasKey("Id");
                    b.HasIndex("CategoryId");
                    b.HasIndex("ProductId");
                    b.ToTable("CommissionPolicy");
                });
            modelBuilder.Entity(
                "Domain.Entities.CommissionPolicyAuditLog",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("Action").IsRequired().HasColumnType("text");
                    b.Property<DateTime>("ChangedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("ChangedByName").IsRequired().HasColumnType("text");
                    b.Property<Guid>("ChangedByUserId").HasColumnType("uuid");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Description").HasColumnType("text");
                    b.Property<string>("NewValueSnapshot").HasColumnType("text");
                    b.Property<string>("OldValueSnapshot").HasColumnType("text");
                    b.Property<int>("PolicyId").HasColumnType("integer");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("PolicyId");
                    b.ToTable("CommissionPolicyAuditLog");
                });
            modelBuilder.Entity(
                "Domain.Entities.CommissionRecord",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTime>("DateEarned").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<int>("EmployeeProfileId").HasColumnType("integer");
                    b.Property<string>("Note").HasColumnType("text");
                    b.Property<int>("OutputId").HasColumnType("integer");
                    b.Property<DateTime?>("PaidAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("PolicySnapshot").HasColumnType("text");
                    b.Property<int>("Status").HasColumnType("integer");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("EmployeeProfileId");
                    b.HasIndex("OutputId");
                    b.ToTable("CommissionRecord");
                });
            modelBuilder.Entity(
                "Domain.Entities.Contact",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Email").IsRequired().HasColumnType("text").HasColumnName("Email");
                    b.Property<string>("FullName").IsRequired().HasColumnType("text").HasColumnName("FullName");
                    b.Property<string>("InternalNote").HasColumnType("text").HasColumnName("InternalNote");
                    b.Property<string>("Message").IsRequired().HasColumnType("text").HasColumnName("Message");
                    b.Property<string>("PhoneNumber").IsRequired().HasColumnType("text").HasColumnName("PhoneNumber");
                    b.Property<int?>("Rating").HasColumnType("integer").HasColumnName("Rating");
                    b.Property<string>("Status").IsRequired().HasColumnType("text").HasColumnName("Status");
                    b.Property<string>("Subject").IsRequired().HasColumnType("text").HasColumnName("Subject");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.ToTable("Contact");
                });
            modelBuilder.Entity(
                "Domain.Entities.ContactReply",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<int>("ContactId").HasColumnType("integer").HasColumnName("ContactId");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Message").IsRequired().HasColumnType("text").HasColumnName("Message");
                    b.Property<Guid>("RepliedById").HasColumnType("uuid").HasColumnName("RepliedById");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("ContactId");
                    b.HasIndex("RepliedById");
                    b.ToTable("ContactReply");
                });
            modelBuilder.Entity(
                "Domain.Entities.EmployeeProfile",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("Address").IsRequired().HasColumnType("text");
                    b.Property<string>("BankAccountNumber").IsRequired().HasColumnType("text");
                    b.Property<string>("BankName").IsRequired().HasColumnType("text");
                    b.Property<decimal>("BaseSalary").HasColumnType("decimal(18,2)");
                    b.Property<DateTime>("ContractDate").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("IdentityNumber").IsRequired().HasColumnType("text");
                    b.Property<string>("JobTitle").IsRequired().HasColumnType("text");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.Property<Guid>("UserId").HasColumnType("uuid");
                    b.HasKey("Id");
                    b.HasIndex("UserId");
                    b.ToTable("EmployeeProfile");
                });
            modelBuilder.Entity(
                "Domain.Entities.Input",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<Guid?>("ConfirmedBy").HasColumnType("uuid").HasColumnName("ConfirmedBy");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<Guid?>("CreatedBy").HasColumnType("uuid").HasColumnName("CreatedBy");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("InputDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("InputDate");
                    b.Property<string>("Notes").HasColumnType("text").HasColumnName("Notes");
                    b.Property<int?>("SourceOrderId").HasColumnType("integer").HasColumnName("SourceOrderId");
                    b.Property<string>("StatusId").HasColumnType("text").HasColumnName("StatusId");
                    b.Property<int?>("SupplierId").HasColumnType("integer").HasColumnName("SupplierId");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("ConfirmedBy");
                    b.HasIndex("CreatedBy");
                    b.HasIndex("SourceOrderId");
                    b.HasIndex("StatusId");
                    b.HasIndex("SupplierId");
                    b.ToTable("Input");
                });
            modelBuilder.Entity(
                "Domain.Entities.InputInfo",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<int?>("Count").HasColumnType("integer").HasColumnName("Count");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<int>("InputId").HasColumnType("integer").HasColumnName("InputId");
                    b.Property<decimal?>("InputPrice").HasColumnType("decimal(18, 2)").HasColumnName("InputPrice");
                    b.Property<int?>("ParentOutputInfoId").HasColumnType("integer").HasColumnName("ParentOutputInfoId");
                    b.Property<int?>("ProductId").HasColumnType("integer").HasColumnName("ProductId");
                    b.Property<int?>("RemainingCount").HasColumnType("integer").HasColumnName("RemainingCount");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("InputId");
                    b.HasIndex("ParentOutputInfoId");
                    b.HasIndex("ProductId");
                    b.ToTable("InputInfo");
                });
            modelBuilder.Entity(
                "Domain.Entities.InputStatus",
                b =>
                {
                    b.Property<string>("Key").HasColumnType("text").HasColumnName("Key");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Key");
                    b.ToTable("InputStatus");
                });
            modelBuilder.Entity(
                "Domain.Entities.KPI",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<decimal>("ActualValue").HasColumnType("decimal(18,2)");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Description").HasColumnType("text");
                    b.Property<int>("EmployeeProfileId").HasColumnType("integer");
                    b.Property<string>("MetricName").IsRequired().HasColumnType("text");
                    b.Property<DateTime>("PeriodEnd").HasColumnType("timestamp with time zone");
                    b.Property<DateTime>("PeriodStart").HasColumnType("timestamp with time zone");
                    b.Property<decimal>("TargetValue").HasColumnType("decimal(18,2)");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("EmployeeProfileId");
                    b.ToTable("KPI");
                });
            modelBuilder.Entity(
                "Domain.Entities.Lead",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("Address").IsRequired().HasColumnType("text").HasColumnName("Address");
                    b.Property<string>("AddressDetail")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("AddressDetail");
                    b.Property<Guid?>("AssignedToId").HasColumnType("uuid").HasColumnName("AssignedToId");
                    b.Property<DateTime?>("Birthday")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("Birthday");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("District").IsRequired().HasColumnType("text").HasColumnName("District");
                    b.Property<string>("Email").IsRequired().HasColumnType("text").HasColumnName("Email");
                    b.Property<string>("FullName").IsRequired().HasColumnType("text").HasColumnName("FullName");
                    b.Property<string>("Gender").IsRequired().HasColumnType("text").HasColumnName("Gender");
                    b.Property<string>("IdentificationNumber")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("IdentificationNumber");
                    b.Property<string>("InterestedVehicle")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("InterestedVehicle");
                    b.Property<string>("PhoneNumber").IsRequired().HasColumnType("text").HasColumnName("PhoneNumber");
                    b.Property<int>("Points").HasColumnType("integer").HasColumnName("Points");
                    b.Property<string>("Province").IsRequired().HasColumnType("text").HasColumnName("Province");
                    b.Property<int>("Score").HasColumnType("integer").HasColumnName("Score");
                    b.Property<string>("Source").IsRequired().HasColumnType("text").HasColumnName("Source");
                    b.Property<string>("Status").IsRequired().HasColumnType("text").HasColumnName("Status");
                    b.Property<string>("Tier").IsRequired().HasColumnType("text").HasColumnName("Tier");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Ward").IsRequired().HasColumnType("text").HasColumnName("Ward");
                    b.HasKey("Id");
                    b.HasIndex("AssignedToId");
                    b.ToTable("Lead");
                });
            modelBuilder.Entity(
                "Domain.Entities.LeadActivity",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("ActivityType").IsRequired().HasColumnType("text").HasColumnName("ActivityType");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Description").IsRequired().HasColumnType("text").HasColumnName("Description");
                    b.Property<int>("LeadId").HasColumnType("integer").HasColumnName("LeadId");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("LeadId");
                    b.ToTable("LeadActivity");
                });
            modelBuilder.Entity(
                "Domain.Entities.MaintenanceHistory",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Description").IsRequired().HasColumnType("text").HasColumnName("Description");
                    b.Property<DateTimeOffset>("MaintenanceDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("MaintenanceDate");
                    b.Property<int>("Mileage").HasColumnType("integer").HasColumnName("Mileage");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.Property<int>("VehicleId").HasColumnType("integer").HasColumnName("VehicleId");
                    b.HasKey("Id");
                    b.HasIndex("VehicleId");
                    b.ToTable("MaintenanceHistory");
                });
            modelBuilder.Entity(
                "Domain.Entities.MediaFile",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("ContentType").HasMaxLength(100).HasColumnType("character varying(100)");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("FileExtension").HasMaxLength(100).HasColumnType("character varying(100)");
                    b.Property<long?>("FileSize").HasColumnType("bigint");
                    b.Property<string>("OriginalFileName").HasMaxLength(255).HasColumnType("character varying(255)");
                    b.Property<string>("StoragePath").HasMaxLength(500).HasColumnType("character varying(500)");
                    b.Property<string>("StorageType")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.ToTable("MediaFiles");
                });
            modelBuilder.Entity(
                "Domain.Entities.News",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<Guid?>("AuthorId").HasColumnType("uuid").HasColumnName("AuthorId");
                    b.Property<string>("AuthorName").HasColumnType("text").HasColumnName("AuthorName");
                    b.Property<int?>("CategoryId").HasColumnType("integer").HasColumnName("CategoryId");
                    b.Property<string>("Content").HasColumnType("text").HasColumnName("Content");
                    b.Property<string>("CoverImageUrl").HasColumnType("text").HasColumnName("CoverImageUrl");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<bool>("IsPublished").HasColumnType("boolean").HasColumnName("IsPublished");
                    b.Property<string>("MetaDescription").HasColumnType("text").HasColumnName("MetaDescription");
                    b.Property<string>("MetaKeywords").HasColumnType("text").HasColumnName("MetaKeywords");
                    b.Property<string>("MetaTitle").HasColumnType("text").HasColumnName("MetaTitle");
                    b.Property<DateTimeOffset?>("PublishedDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("PublishedDate");
                    b.Property<string>("Slug").IsRequired().HasColumnType("varchar(255)").HasColumnName("Slug");
                    b.Property<string>("Title").IsRequired().HasColumnType("text").HasColumnName("Title");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("AuthorId");
                    b.HasIndex("CategoryId");
                    b.ToTable("News");
                });
            modelBuilder.Entity(
                "Domain.Entities.NewsCategory",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<bool>("IsActive").HasColumnType("boolean").HasColumnName("IsActive");
                    b.Property<string>("Name").IsRequired().HasColumnType("text").HasColumnName("Name");
                    b.Property<string>("Slug").IsRequired().HasColumnType("varchar(255)").HasColumnName("Slug");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.ToTable("NewsCategory");
                });
            modelBuilder.Entity(
                "Domain.Entities.Option",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Name").HasColumnType("text").HasColumnName("Name");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("Name");
                    b.ToTable("Option");
                });
            modelBuilder.Entity(
                "Domain.Entities.OptionValue",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("ColorCode").HasColumnType("text").HasColumnName("ColorCode");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Description").HasColumnType("text").HasColumnName("Description");
                    b.Property<string>("ImageUrl").HasColumnType("text").HasColumnName("ImageUrl");
                    b.Property<bool>("IsActive").HasColumnType("boolean").HasColumnName("IsActive");
                    b.Property<string>("Name").HasColumnType("text").HasColumnName("Name");
                    b.Property<int?>("OptionId").HasColumnType("integer").HasColumnName("OptionId");
                    b.Property<string>("SeoDescription").HasColumnType("text").HasColumnName("SeoDescription");
                    b.Property<string>("SeoTitle").HasColumnType("text").HasColumnName("SeoTitle");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("OptionId");
                    b.ToTable("OptionValue");
                });
            modelBuilder.Entity(
                "Domain.Entities.Output",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<Guid?>("BuyerId").HasColumnType("uuid").HasColumnName("BuyerId");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<Guid?>("CreatedBy").HasColumnType("uuid").HasColumnName("CreatedBy");
                    b.Property<string>("CustomerAddress").HasColumnType("text").HasColumnName("CustomerAddress");
                    b.Property<string>("CustomerName").HasColumnType("text").HasColumnName("CustomerName");
                    b.Property<string>("CustomerPhone").HasColumnType("text").HasColumnName("CustomerPhone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<int?>("DepositRatio").HasColumnType("integer").HasColumnName("DepositRatio");
                    b.Property<Guid?>("FinishedBy").HasColumnType("uuid").HasColumnName("FinishedBy");
                    b.Property<DateTimeOffset?>("LastStatusChangedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("LastStatusChangedAt");
                    b.Property<string>("Notes").HasColumnType("text").HasColumnName("Notes");
                    b.Property<decimal?>("PaidAmount").HasColumnType("decimal(18, 2)").HasColumnName("PaidAmount");
                    b.Property<DateTimeOffset?>("PaidAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("PaidAt");
                    b.Property<string>("PaymentCode").HasColumnType("text").HasColumnName("PaymentCode");
                    b.Property<DateTimeOffset?>("PaymentExpiredAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("PaymentExpiredAt");
                    b.Property<string>("PaymentMethod").HasColumnType("text").HasColumnName("PaymentMethod");
                    b.Property<string>("PaymentStatus").HasColumnType("text").HasColumnName("PaymentStatus");
                    b.Property<string>("PaymentUrl").HasColumnType("text").HasColumnName("PaymentUrl");
                    b.Property<string>("StatusId").HasColumnType("text").HasColumnName("StatusId");
                    b.Property<string>("TransactionId").HasColumnType("text").HasColumnName("TransactionId");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("BuyerId");
                    b.HasIndex("CreatedBy");
                    b.HasIndex("FinishedBy");
                    b.HasIndex("StatusId");
                    b.ToTable("Output");
                });
            modelBuilder.Entity(
                "Domain.Entities.OutputInfo",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<decimal?>("CostPrice").HasColumnType("decimal(18, 2)").HasColumnName("CostPrice");
                    b.Property<int?>("Count").HasColumnType("integer").HasColumnName("Count");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<int>("OutputId").HasColumnType("integer").HasColumnName("OutputId");
                    b.Property<decimal?>("Price").HasColumnType("decimal(18, 2)").HasColumnName("Price");
                    b.Property<int?>("ProductVarientId").HasColumnType("integer").HasColumnName("ProductVarientId");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("OutputId");
                    b.HasIndex("ProductVarientId");
                    b.ToTable("OutputInfo");
                });
            modelBuilder.Entity(
                "Domain.Entities.OutputStatus",
                b =>
                {
                    b.Property<string>("Key").HasColumnType("text").HasColumnName("Key");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Key");
                    b.ToTable("OutputStatus");
                });
            modelBuilder.Entity(
                "Domain.Entities.PartnerType",
                b =>
                {
                    b.Property<string>("Key").HasColumnType("text").HasColumnName("Key");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Key");
                    b.ToTable("PartnerType");
                    b.HasData(new { Key = "supplier" }, new { Key = "financial" }, new { Key = "insurance" });
                });
            modelBuilder.Entity(
                "Domain.Entities.Payroll",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<DateTime?>("ApprovedAt").HasColumnType("timestamp with time zone");
                    b.Property<Guid?>("ApprovedBy").HasColumnType("uuid");
                    b.Property<decimal>("BaseSalary").HasColumnType("decimal(18,2)");
                    b.Property<decimal>("Bonus").HasColumnType("decimal(18,2)");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<int>("EmployeeProfileId").HasColumnType("integer");
                    b.Property<bool>("IsApproved").HasColumnType("boolean");
                    b.Property<int>("Month").HasColumnType("integer");
                    b.Property<decimal>("Penalty").HasColumnType("decimal(18,2)");
                    b.Property<decimal>("TotalCommission").HasColumnType("decimal(18,2)");
                    b.Property<decimal>("TotalSalary").HasColumnType("decimal(18,2)");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.Property<int>("Year").HasColumnType("integer");
                    b.HasKey("Id");
                    b.HasIndex("EmployeeProfileId");
                    b.ToTable("Payroll");
                });
            modelBuilder.Entity(
                "Domain.Entities.Permission",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("Name").IsRequired().HasColumnType("text");
                    b.HasKey("Id");
                    b.ToTable("Permissions", (string)null);
                });
            modelBuilder.Entity(
                "Domain.Entities.PredefinedOption",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Key").IsRequired().HasColumnType("text").HasColumnName("Key");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Value").IsRequired().HasColumnType("text").HasColumnName("Value");
                    b.HasKey("Id");
                    b.HasIndex("Key").IsUnique();
                    b.ToTable("PredefinedOption");
                });
            modelBuilder.Entity(
                "Domain.Entities.Product",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("BatteryType").HasColumnType("text").HasColumnName("BatteryType");
                    b.Property<string>("BoreStroke").HasColumnType("text").HasColumnName("BoreStroke");
                    b.Property<int?>("BrandId").HasColumnType("integer").HasColumnName("BrandId");
                    b.Property<int?>("CategoryId").HasColumnType("integer").HasColumnName("CategoryId");
                    b.Property<string>("CompressionRatio").HasColumnType("text").HasColumnName("CompressionRatio");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("DashboardType").HasColumnType("text").HasColumnName("DashboardType");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Description").HasColumnType("text").HasColumnName("Description");
                    b.Property<string>("Dimensions").HasColumnType("text").HasColumnName("Dimensions");
                    b.Property<decimal?>("Displacement").HasColumnType("numeric").HasColumnName("Displacement");
                    b.Property<string>("EngineType").HasColumnType("text").HasColumnName("EngineType");
                    b.Property<string>("FrameType").HasColumnType("text").HasColumnName("FrameType");
                    b.Property<string>("FrontBrake").HasColumnType("text").HasColumnName("FrontBrake");
                    b.Property<string>("FrontSuspension").HasColumnType("text").HasColumnName("FrontSuspension");
                    b.Property<string>("FrontTireSize").HasColumnType("text").HasColumnName("FrontTireSize");
                    b.Property<decimal?>("FuelCapacity").HasColumnType("numeric").HasColumnName("FuelCapacity");
                    b.Property<string>("FuelConsumption").HasColumnType("text").HasColumnName("FuelConsumption");
                    b.Property<string>("FuelSystem").HasColumnType("text").HasColumnName("FuelSystem");
                    b.Property<decimal?>("GroundClearance").HasColumnType("numeric").HasColumnName("GroundClearance");
                    b.Property<string>("Highlights").HasColumnType("text").HasColumnName("Highlights");
                    b.Property<string>("LightingSystem").HasColumnType("text").HasColumnName("LightingSystem");
                    b.Property<string>("Material").HasColumnType("text").HasColumnName("Material");
                    b.Property<string>("MaxPower").HasColumnType("text").HasColumnName("MaxPower");
                    b.Property<string>("MaxTorque").HasColumnType("text").HasColumnName("MaxTorque");
                    b.Property<string>("MetaDescription").HasColumnType("text").HasColumnName("MetaDescription");
                    b.Property<string>("MetaTitle").HasColumnType("text").HasColumnName("MetaTitle");
                    b.Property<string>("Name").HasColumnType("text").HasColumnName("Name");
                    b.Property<decimal?>("OilCapacity").HasColumnType("numeric").HasColumnName("OilCapacity");
                    b.Property<string>("Origin").HasColumnType("text").HasColumnName("Origin");
                    b.Property<string>("OtherStandards").HasColumnType("text").HasColumnName("OtherStandards");
                    b.Property<string>("RearBrake").HasColumnType("text").HasColumnName("RearBrake");
                    b.Property<string>("RearSuspension").HasColumnType("text").HasColumnName("RearSuspension");
                    b.Property<string>("RearTireSize").HasColumnType("text").HasColumnName("RearTireSize");
                    b.Property<decimal?>("SeatHeight").HasColumnType("numeric").HasColumnName("SeatHeight");
                    b.Property<string>("ShortDescription").HasColumnType("text").HasColumnName("ShortDescription");
                    b.Property<string>("StarterSystem").HasColumnType("text").HasColumnName("StarterSystem");
                    b.Property<string>("StatusId").HasColumnType("text").HasColumnName("StatusId");
                    b.Property<bool>("StdDot").HasColumnType("boolean").HasColumnName("StdDot");
                    b.Property<bool>("StdEce").HasColumnType("boolean").HasColumnName("StdEce");
                    b.Property<bool>("StdJis").HasColumnType("boolean").HasColumnName("StdJis");
                    b.Property<bool>("StdSnell").HasColumnType("boolean").HasColumnName("StdSnell");
                    b.Property<string>("TireSize").HasColumnType("text").HasColumnName("TireSize");
                    b.Property<string>("TransmissionType").HasColumnType("text").HasColumnName("TransmissionType");
                    b.Property<string>("Unit").HasColumnType("text").HasColumnName("Unit");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("WarrantyPeriod").HasColumnType("text").HasColumnName("WarrantyPeriod");
                    b.Property<decimal?>("Weight").HasColumnType("numeric").HasColumnName("Weight");
                    b.Property<string>("Wheelbase").HasColumnType("text").HasColumnName("Wheelbase");
                    b.HasKey("Id");
                    b.HasIndex("BrandId");
                    b.HasIndex("CategoryId");
                    b.HasIndex("StatusId");
                    b.ToTable("Product");
                });
            modelBuilder.Entity(
                "Domain.Entities.ProductCategory",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("CategoryGroup").HasColumnType("text").HasColumnName("CategoryGroup");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Description").HasColumnType("text").HasColumnName("Description");
                    b.Property<string>("ImageUrl").HasColumnType("text").HasColumnName("ImageUrl");
                    b.Property<bool>("IsActive").HasColumnType("boolean").HasColumnName("IsActive");
                    b.Property<int?>("MaxPurchaseQuantity")
                        .HasColumnType("integer")
                        .HasColumnName("MaxPurchaseQuantity");
                    b.Property<string>("Name").HasColumnType("text").HasColumnName("Name");
                    b.Property<int?>("ParentId").HasColumnType("integer").HasColumnName("ParentId");
                    b.Property<string>("Slug").HasColumnType("text").HasColumnName("Slug");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("ParentId");
                    b.ToTable("ProductCategory");
                });
            modelBuilder.Entity(
                "Domain.Entities.ProductCollectionPhoto",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("ImageUrl").HasColumnType("text").HasColumnName("ImageUrl");
                    b.Property<int>("ProductVariantId").HasColumnType("integer").HasColumnName("ProductVariantId");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("ProductVariantId");
                    b.ToTable("ProductCollectionPhoto");
                });
            modelBuilder.Entity(
                "Domain.Entities.ProductCompatibility",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<int>("BaseProductId").HasColumnType("integer").HasColumnName("BaseProductId");
                    b.Property<int>("CompatibleVehicleModelId")
                        .HasColumnType("integer")
                        .HasColumnName("CompatibleVehicleModelId");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Notes").HasColumnType("text").HasColumnName("Notes");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("BaseProductId");
                    b.HasIndex("CompatibleVehicleModelId");
                    b.ToTable("ProductCompatibility");
                });
            modelBuilder.Entity(
                "Domain.Entities.ProductStatus",
                b =>
                {
                    b.Property<string>("Key").HasColumnType("text").HasColumnName("Key");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Key");
                    b.ToTable("ProductStatus");
                    b.HasData(new { Key = "for-sale" }, new { Key = "out-of-business" });
                });
            modelBuilder.Entity(
                "Domain.Entities.ProductTechnology",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("CustomDescription").HasColumnType("text").HasColumnName("CustomDescription");
                    b.Property<string>("CustomImageUrl").HasColumnType("text").HasColumnName("CustomImageUrl");
                    b.Property<string>("CustomTitle").HasColumnType("text").HasColumnName("CustomTitle");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<int>("DisplayOrder").HasColumnType("integer").HasColumnName("DisplayOrder");
                    b.Property<int>("ProductId").HasColumnType("integer").HasColumnName("ProductId");
                    b.Property<int>("TechnologyId").HasColumnType("integer").HasColumnName("TechnologyId");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("ProductId");
                    b.HasIndex("TechnologyId");
                    b.ToTable("ProductTechnology");
                });
            modelBuilder.Entity(
                "Domain.Entities.ProductVariant",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("ColorCode").HasColumnType("text").HasColumnName("ColorCode");
                    b.Property<string>("ColorName").HasColumnType("text").HasColumnName("ColorName");
                    b.Property<string>("CoverImageUrl").HasColumnType("text").HasColumnName("CoverImageUrl");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Dimensions").HasColumnType("text").HasColumnName("Dimensions");
                    b.Property<string>("EngineType").HasColumnType("text").HasColumnName("EngineType");
                    b.Property<string>("FrontBrake").HasColumnType("text").HasColumnName("FrontBrake");
                    b.Property<string>("FrontSuspension").HasColumnType("text").HasColumnName("FrontSuspension");
                    b.Property<decimal?>("FuelCapacity").HasColumnType("decimal(18, 2)").HasColumnName("FuelCapacity");
                    b.Property<decimal?>("GroundClearance")
                        .HasColumnType("decimal(18, 2)")
                        .HasColumnName("GroundClearance");
                    b.Property<decimal?>("Price").HasColumnType("decimal(18, 2)").HasColumnName("Price");
                    b.Property<int>("ProductId").HasColumnType("integer").HasColumnName("ProductId");
                    b.Property<string>("RearBrake").HasColumnType("text").HasColumnName("RearBrake");
                    b.Property<string>("RearSuspension").HasColumnType("text").HasColumnName("RearSuspension");
                    b.Property<string>("SKU").HasColumnType("text").HasColumnName("SKU");
                    b.Property<decimal?>("SeatHeight").HasColumnType("decimal(18, 2)").HasColumnName("SeatHeight");
                    b.Property<string>("TireSize").HasColumnType("text").HasColumnName("TireSize");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("UrlSlug").HasColumnType("text").HasColumnName("UrlSlug");
                    b.Property<string>("VersionName").HasColumnType("text").HasColumnName("VersionName");
                    b.Property<decimal?>("Weight").HasColumnType("decimal(18, 2)").HasColumnName("Weight");
                    b.Property<decimal?>("Wheelbase").HasColumnType("decimal(18, 2)").HasColumnName("Wheelbase");
                    b.HasKey("Id");
                    b.HasIndex("ProductId");
                    b.ToTable("ProductVariant");
                });
            modelBuilder.Entity(
                "Domain.Entities.RolePermission",
                b =>
                {
                    b.Property<Guid>("RoleId").HasColumnType("uuid");
                    b.Property<int>("PermissionId").HasColumnType("integer");
                    b.HasKey("RoleId", "PermissionId");
                    b.HasIndex("PermissionId");
                    b.ToTable("RolePermissions", (string)null);
                });
            modelBuilder.Entity(
                "Domain.Entities.Setting",
                b =>
                {
                    b.Property<string>("Key").HasColumnType("text").HasColumnName("Key");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Value").HasColumnType("text").HasColumnName("Value");
                    b.HasKey("Key");
                    b.ToTable("Setting");
                });
            modelBuilder.Entity(
                "Domain.Entities.Supplier",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("Address").HasColumnType("text").HasColumnName("Address");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Email").HasColumnType("text").HasColumnName("Email");
                    b.Property<string>("Name").HasColumnType("text").HasColumnName("Name");
                    b.Property<string>("Notes").HasColumnType("text").HasColumnName("Notes");
                    b.Property<string>("PartnerTypeId").HasColumnType("text").HasColumnName("PartnerTypeId");
                    b.Property<string>("Phone").HasColumnType("text").HasColumnName("Phone");
                    b.Property<string>("StatusId").HasColumnType("text").HasColumnName("StatusId");
                    b.Property<string>("TaxIdentificationNumber")
                        .HasColumnType("varchar(20)")
                        .HasColumnName("TaxIdentificationNumber");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("PartnerTypeId");
                    b.HasIndex("StatusId");
                    b.ToTable("Supplier");
                });
            modelBuilder.Entity(
                "Domain.Entities.SupplierContact",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("CitizenID").HasColumnType("varchar(20)").HasColumnName("CitizenID");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Email").HasColumnType("text").HasColumnName("Email");
                    b.Property<string>("Name").HasColumnType("text").HasColumnName("Name");
                    b.Property<string>("Phone").HasColumnType("text").HasColumnName("Phone");
                    b.Property<int?>("SupplierId").HasColumnType("integer").HasColumnName("SupplierId");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("SupplierId");
                    b.ToTable("SupplierContact");
                });
            modelBuilder.Entity(
                "Domain.Entities.SupplierStatus",
                b =>
                {
                    b.Property<string>("Key").HasColumnType("text").HasColumnName("Key");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Key");
                    b.ToTable("SupplierStatus");
                });
            modelBuilder.Entity(
                "Domain.Entities.Technology",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<int?>("BrandId").HasColumnType("integer");
                    b.Property<int?>("CategoryId").HasColumnType("integer");
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("DefaultDescription").HasColumnType("text").HasColumnName("DefaultDescription");
                    b.Property<string>("DefaultImageUrl").HasColumnType("text").HasColumnName("DefaultImageUrl");
                    b.Property<string>("DefaultTitle").HasColumnType("text").HasColumnName("DefaultTitle");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Name").IsRequired().HasColumnType("text").HasColumnName("Name");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("BrandId");
                    b.HasIndex("CategoryId");
                    b.ToTable("Technologies");
                });
            modelBuilder.Entity(
                "Domain.Entities.TechnologyCategory",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Name").IsRequired().HasColumnType("text").HasColumnName("Name");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.ToTable("TechnologyCategories");
                });
            modelBuilder.Entity(
                "Domain.Entities.TechnologyImage",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("ImageUrl").IsRequired().HasColumnType("text").HasColumnName("ImageUrl");
                    b.Property<int>("TechnologyId").HasColumnType("integer");
                    b.Property<string>("Type").IsRequired().HasColumnType("text").HasColumnName("Type");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.HasKey("Id");
                    b.HasIndex("TechnologyId");
                    b.ToTable("TechnologyImages");
                });
            modelBuilder.Entity(
                "Domain.Entities.VariantOptionValue",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<int?>("OptionValueId").HasColumnType("integer").HasColumnName("OptionValueId");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.Property<int>("VariantId").HasColumnType("integer").HasColumnName("VariantId");
                    b.HasKey("Id");
                    b.HasIndex("OptionValueId");
                    b.HasIndex("VariantId");
                    b.ToTable("VariantOptionValue");
                });
            modelBuilder.Entity(
                "Domain.Entities.Vehicle",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("EngineNumber").IsRequired().HasColumnType("text").HasColumnName("EngineNumber");
                    b.Property<bool>("IsActive").HasColumnType("boolean").HasColumnName("IsActive");
                    b.Property<int>("LeadId").HasColumnType("integer").HasColumnName("LeadId");
                    b.Property<string>("LicensePlate").IsRequired().HasColumnType("text").HasColumnName("LicensePlate");
                    b.Property<int?>("ProductId").HasColumnType("integer").HasColumnName("ProductId");
                    b.Property<DateTimeOffset>("PurchaseDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("PurchaseDate");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("VinNumber").IsRequired().HasColumnType("text").HasColumnName("VinNumber");
                    b.HasKey("Id");
                    b.HasIndex("LeadId");
                    b.HasIndex("ProductId");
                    b.ToTable("Vehicle");
                });
            modelBuilder.Entity(
                "Domain.Entities.VehicleDocument",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasColumnName("Id");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<DateTimeOffset?>("CreatedAt").HasColumnType("timestamp with time zone");
                    b.Property<DateTimeOffset?>("DeletedAt").HasColumnType("timestamp with time zone");
                    b.Property<string>("Description").IsRequired().HasColumnType("text").HasColumnName("Description");
                    b.Property<string>("DocumentType").IsRequired().HasColumnType("text").HasColumnName("DocumentType");
                    b.Property<string>("FileUrl").IsRequired().HasColumnType("text").HasColumnName("FileUrl");
                    b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("timestamp with time zone");
                    b.Property<int>("VehicleId").HasColumnType("integer").HasColumnName("VehicleId");
                    b.HasKey("Id");
                    b.HasIndex("VehicleId");
                    b.ToTable("VehicleDocument");
                });
            modelBuilder.Entity(
                "Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("ClaimType").HasColumnType("text");
                    b.Property<string>("ClaimValue").HasColumnType("text");
                    b.Property<Guid>("RoleId").HasColumnType("uuid");
                    b.HasKey("Id");
                    b.HasIndex("RoleId");
                    b.ToTable("RoleClaims", (string)null);
                });
            modelBuilder.Entity(
                "Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>",
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                    b.Property<string>("ClaimType").HasColumnType("text");
                    b.Property<string>("ClaimValue").HasColumnType("text");
                    b.Property<Guid>("UserId").HasColumnType("uuid");
                    b.HasKey("Id");
                    b.HasIndex("UserId");
                    b.ToTable("UserClaims", (string)null);
                });
            modelBuilder.Entity(
                "Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>",
                b =>
                {
                    b.Property<string>("LoginProvider").HasColumnType("text");
                    b.Property<string>("ProviderKey").HasColumnType("text");
                    b.Property<string>("ProviderDisplayName").HasColumnType("text");
                    b.Property<Guid>("UserId").HasColumnType("uuid");
                    b.HasKey("LoginProvider", "ProviderKey");
                    b.HasIndex("UserId");
                    b.ToTable("UserLogins", (string)null);
                });
            modelBuilder.Entity(
                "Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>",
                b =>
                {
                    b.Property<Guid>("UserId").HasColumnType("uuid");
                    b.Property<Guid>("RoleId").HasColumnType("uuid");
                    b.Property<Guid?>("ApplicationUserId").HasColumnType("uuid");
                    b.HasKey("UserId", "RoleId");
                    b.HasIndex("ApplicationUserId");
                    b.HasIndex("RoleId");
                    b.ToTable("UserRoles", (string)null);
                });
            modelBuilder.Entity(
                "Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>",
                b =>
                {
                    b.Property<Guid>("UserId").HasColumnType("uuid");
                    b.Property<string>("LoginProvider").HasColumnType("text");
                    b.Property<string>("Name").HasColumnType("text");
                    b.Property<string>("Value").HasColumnType("text");
                    b.HasKey("UserId", "LoginProvider", "Name");
                    b.ToTable("UserTokens", (string)null);
                });
            modelBuilder.Entity(
                "Domain.Entities.BannerAuditLog",
                b =>
                {
                    b.HasOne("Domain.Entities.Banner", "Banner")
                        .WithMany()
                        .HasForeignKey("BannerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("Banner");
                });
            modelBuilder.Entity(
                "Domain.Entities.Booking",
                b =>
                {
                    b.HasOne("Domain.Entities.ProductVariant", "ProductVariant")
                        .WithMany()
                        .HasForeignKey("ProductVariantId");
                    b.Navigation("ProductVariant");
                });
            modelBuilder.Entity(
                "Domain.Entities.CommissionPolicy",
                b =>
                {
                    b.HasOne("Domain.Entities.ProductCategory", "Category").WithMany().HasForeignKey("CategoryId");
                    b.HasOne("Domain.Entities.Product", "Product").WithMany().HasForeignKey("ProductId");
                    b.Navigation("Category");
                    b.Navigation("Product");
                });
            modelBuilder.Entity(
                "Domain.Entities.CommissionPolicyAuditLog",
                b =>
                {
                    b.HasOne("Domain.Entities.CommissionPolicy", "Policy")
                        .WithMany()
                        .HasForeignKey("PolicyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("Policy");
                });
            modelBuilder.Entity(
                "Domain.Entities.CommissionRecord",
                b =>
                {
                    b.HasOne("Domain.Entities.EmployeeProfile", "EmployeeProfile")
                        .WithMany("CommissionRecords")
                        .HasForeignKey("EmployeeProfileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.HasOne("Domain.Entities.Output", "Output")
                        .WithMany()
                        .HasForeignKey("OutputId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("EmployeeProfile");
                    b.Navigation("Output");
                });
            modelBuilder.Entity(
                "Domain.Entities.ContactReply",
                b =>
                {
                    b.HasOne("Domain.Entities.Contact", "Contact")
                        .WithMany("Replies")
                        .HasForeignKey("ContactId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.HasOne("Domain.Entities.ApplicationUser", "RepliedBy")
                        .WithMany()
                        .HasForeignKey("RepliedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("Contact");
                    b.Navigation("RepliedBy");
                });
            modelBuilder.Entity(
                "Domain.Entities.EmployeeProfile",
                b =>
                {
                    b.HasOne("Domain.Entities.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("User");
                });
            modelBuilder.Entity(
                "Domain.Entities.Input",
                b =>
                {
                    b.HasOne("Domain.Entities.ApplicationUser", "ConfirmedByUser")
                        .WithMany()
                        .HasForeignKey("ConfirmedBy");
                    b.HasOne("Domain.Entities.ApplicationUser", "CreatedByUser").WithMany().HasForeignKey("CreatedBy");
                    b.HasOne("Domain.Entities.Output", "Output").WithMany("Returns").HasForeignKey("SourceOrderId");
                    b.HasOne("Domain.Entities.InputStatus", "InputStatus")
                        .WithMany("InputReceipts")
                        .HasForeignKey("StatusId");
                    b.HasOne("Domain.Entities.Supplier", "Supplier")
                        .WithMany("InputReceipts")
                        .HasForeignKey("SupplierId");
                    b.Navigation("ConfirmedByUser");
                    b.Navigation("CreatedByUser");
                    b.Navigation("InputStatus");
                    b.Navigation("Output");
                    b.Navigation("Supplier");
                });
            modelBuilder.Entity(
                "Domain.Entities.InputInfo",
                b =>
                {
                    b.HasOne("Domain.Entities.Input", "InputReceipt")
                        .WithMany("InputInfos")
                        .HasForeignKey("InputId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.HasOne("Domain.Entities.OutputInfo", "ParentOutputInfo")
                        .WithMany("Returns")
                        .HasForeignKey("ParentOutputInfoId");
                    b.HasOne("Domain.Entities.ProductVariant", "ProductVariant")
                        .WithMany("InputInfos")
                        .HasForeignKey("ProductId");
                    b.Navigation("InputReceipt");
                    b.Navigation("ParentOutputInfo");
                    b.Navigation("ProductVariant");
                });
            modelBuilder.Entity(
                "Domain.Entities.KPI",
                b =>
                {
                    b.HasOne("Domain.Entities.EmployeeProfile", "EmployeeProfile")
                        .WithMany("KPIs")
                        .HasForeignKey("EmployeeProfileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("EmployeeProfile");
                });
            modelBuilder.Entity(
                "Domain.Entities.Lead",
                b =>
                {
                    b.HasOne("Domain.Entities.ApplicationUser", "AssignedTo").WithMany().HasForeignKey("AssignedToId");
                    b.Navigation("AssignedTo");
                });
            modelBuilder.Entity(
                "Domain.Entities.LeadActivity",
                b =>
                {
                    b.HasOne("Domain.Entities.Lead", "Lead")
                        .WithMany("Activities")
                        .HasForeignKey("LeadId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("Lead");
                });
            modelBuilder.Entity(
                "Domain.Entities.MaintenanceHistory",
                b =>
                {
                    b.HasOne("Domain.Entities.Vehicle", "Vehicle")
                        .WithMany("MaintenanceHistories")
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("Vehicle");
                });
            modelBuilder.Entity(
                "Domain.Entities.News",
                b =>
                {
                    b.HasOne("Domain.Entities.ApplicationUser", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Restrict);
                    b.HasOne("Domain.Entities.NewsCategory", "Category")
                        .WithMany("News")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.SetNull);
                    b.Navigation("Author");
                    b.Navigation("Category");
                });
            modelBuilder.Entity(
                "Domain.Entities.Option",
                b =>
                {
                    b.HasOne("Domain.Entities.PredefinedOption", null)
                        .WithMany()
                        .HasForeignKey("Name")
                        .HasPrincipalKey("Key")
                        .OnDelete(DeleteBehavior.Restrict);
                });
            modelBuilder.Entity(
                "Domain.Entities.OptionValue",
                b =>
                {
                    b.HasOne("Domain.Entities.Option", "Option").WithMany("OptionValues").HasForeignKey("OptionId");
                    b.Navigation("Option");
                });
            modelBuilder.Entity(
                "Domain.Entities.Output",
                b =>
                {
                    b.HasOne("Domain.Entities.ApplicationUser", "Buyer").WithMany().HasForeignKey("BuyerId");
                    b.HasOne("Domain.Entities.ApplicationUser", "CreatedByUser").WithMany().HasForeignKey("CreatedBy");
                    b.HasOne("Domain.Entities.ApplicationUser", "FinishedByUser").WithMany().HasForeignKey("FinishedBy");
                    b.HasOne("Domain.Entities.OutputStatus", "OutputStatus")
                        .WithMany("OutputOrders")
                        .HasForeignKey("StatusId");
                    b.Navigation("Buyer");
                    b.Navigation("CreatedByUser");
                    b.Navigation("FinishedByUser");
                    b.Navigation("OutputStatus");
                });
            modelBuilder.Entity(
                "Domain.Entities.OutputInfo",
                b =>
                {
                    b.HasOne("Domain.Entities.Output", "OutputOrder")
                        .WithMany("OutputInfos")
                        .HasForeignKey("OutputId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.HasOne("Domain.Entities.ProductVariant", "ProductVariant")
                        .WithMany("OutputInfos")
                        .HasForeignKey("ProductVarientId");
                    b.Navigation("OutputOrder");
                    b.Navigation("ProductVariant");
                });
            modelBuilder.Entity(
                "Domain.Entities.Payroll",
                b =>
                {
                    b.HasOne("Domain.Entities.EmployeeProfile", "EmployeeProfile")
                        .WithMany()
                        .HasForeignKey("EmployeeProfileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("EmployeeProfile");
                });
            modelBuilder.Entity(
                "Domain.Entities.Product",
                b =>
                {
                    b.HasOne("Domain.Entities.Brand", "Brand").WithMany("Products").HasForeignKey("BrandId");
                    b.HasOne("Domain.Entities.ProductCategory", "ProductCategory")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId");
                    b.HasOne("Domain.Entities.ProductStatus", "ProductStatus")
                        .WithMany("Products")
                        .HasForeignKey("StatusId");
                    b.Navigation("Brand");
                    b.Navigation("ProductCategory");
                    b.Navigation("ProductStatus");
                });
            modelBuilder.Entity(
                "Domain.Entities.ProductCategory",
                b =>
                {
                    b.HasOne("Domain.Entities.ProductCategory", "Parent")
                        .WithMany("SubCategories")
                        .HasForeignKey("ParentId");
                    b.Navigation("Parent");
                });
            modelBuilder.Entity(
                "Domain.Entities.ProductCollectionPhoto",
                b =>
                {
                    b.HasOne("Domain.Entities.ProductVariant", "ProductVariant")
                        .WithMany("ProductCollectionPhotos")
                        .HasForeignKey("ProductVariantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("ProductVariant");
                });
            modelBuilder.Entity(
                "Domain.Entities.ProductCompatibility",
                b =>
                {
                    b.HasOne("Domain.Entities.Product", "BaseProduct")
                        .WithMany("CompatibleWith")
                        .HasForeignKey("BaseProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.HasOne("Domain.Entities.Product", "CompatibleVehicleModel")
                        .WithMany("SupportedBy")
                        .HasForeignKey("CompatibleVehicleModelId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                    b.Navigation("BaseProduct");
                    b.Navigation("CompatibleVehicleModel");
                });
            modelBuilder.Entity(
                "Domain.Entities.ProductTechnology",
                b =>
                {
                    b.HasOne("Domain.Entities.Product", "Product")
                        .WithMany("ProductTechnologies")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.HasOne("Domain.Entities.Technology", "Technology")
                        .WithMany("ProductTechnologies")
                        .HasForeignKey("TechnologyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("Product");
                    b.Navigation("Technology");
                });
            modelBuilder.Entity(
                "Domain.Entities.ProductVariant",
                b =>
                {
                    b.HasOne("Domain.Entities.Product", "Product")
                        .WithMany("ProductVariants")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("Product");
                });
            modelBuilder.Entity(
                "Domain.Entities.RolePermission",
                b =>
                {
                    b.HasOne("Domain.Entities.Permission", "Permission")
                        .WithMany("RolePermissions")
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.HasOne("Domain.Entities.ApplicationRole", "Role")
                        .WithMany("RolePermissions")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("Permission");
                    b.Navigation("Role");
                });
            modelBuilder.Entity(
                "Domain.Entities.Supplier",
                b =>
                {
                    b.HasOne("Domain.Entities.PartnerType", "PartnerType")
                        .WithMany("Suppliers")
                        .HasForeignKey("PartnerTypeId");
                    b.HasOne("Domain.Entities.SupplierStatus", "SupplierStatus")
                        .WithMany("Suppliers")
                        .HasForeignKey("StatusId");
                    b.Navigation("PartnerType");
                    b.Navigation("SupplierStatus");
                });
            modelBuilder.Entity(
                "Domain.Entities.SupplierContact",
                b =>
                {
                    b.HasOne("Domain.Entities.Supplier", "Supplier")
                        .WithMany("SupplierContacts")
                        .HasForeignKey("SupplierId");
                    b.Navigation("Supplier");
                });
            modelBuilder.Entity(
                "Domain.Entities.Technology",
                b =>
                {
                    b.HasOne("Domain.Entities.Brand", "Brand").WithMany().HasForeignKey("BrandId");
                    b.HasOne("Domain.Entities.TechnologyCategory", "Category")
                        .WithMany("Technologies")
                        .HasForeignKey("CategoryId");
                    b.Navigation("Brand");
                    b.Navigation("Category");
                });
            modelBuilder.Entity(
                "Domain.Entities.TechnologyImage",
                b =>
                {
                    b.HasOne("Domain.Entities.Technology", "Technology")
                        .WithMany("Images")
                        .HasForeignKey("TechnologyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("Technology");
                });
            modelBuilder.Entity(
                "Domain.Entities.VariantOptionValue",
                b =>
                {
                    b.HasOne("Domain.Entities.OptionValue", "OptionValue")
                        .WithMany("VariantOptionValues")
                        .HasForeignKey("OptionValueId");
                    b.HasOne("Domain.Entities.ProductVariant", "ProductVariant")
                        .WithMany("VariantOptionValues")
                        .HasForeignKey("VariantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("OptionValue");
                    b.Navigation("ProductVariant");
                });
            modelBuilder.Entity(
                "Domain.Entities.Vehicle",
                b =>
                {
                    b.HasOne("Domain.Entities.Lead", "Lead")
                        .WithMany()
                        .HasForeignKey("LeadId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.HasOne("Domain.Entities.Product", "Product").WithMany().HasForeignKey("ProductId");
                    b.Navigation("Lead");
                    b.Navigation("Product");
                });
            modelBuilder.Entity(
                "Domain.Entities.VehicleDocument",
                b =>
                {
                    b.HasOne("Domain.Entities.Vehicle", "Vehicle")
                        .WithMany("Documents")
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.Navigation("Vehicle");
                });
            modelBuilder.Entity(
                "Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>",
                b =>
                {
                    b.HasOne("Domain.Entities.ApplicationRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
            modelBuilder.Entity(
                "Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>",
                b =>
                {
                    b.HasOne("Domain.Entities.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
            modelBuilder.Entity(
                "Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>",
                b =>
                {
                    b.HasOne("Domain.Entities.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
            modelBuilder.Entity(
                "Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>",
                b =>
                {
                    b.HasOne("Domain.Entities.ApplicationUser", null)
                        .WithMany("UserRoles")
                        .HasForeignKey("ApplicationUserId");
                    b.HasOne("Domain.Entities.ApplicationRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                    b.HasOne("Domain.Entities.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
            modelBuilder.Entity(
                "Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>",
                b =>
                {
                    b.HasOne("Domain.Entities.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
            modelBuilder.Entity(
                "Domain.Entities.ApplicationRole",
                b =>
                {
                    b.Navigation("RolePermissions");
                });
            modelBuilder.Entity(
                "Domain.Entities.ApplicationUser",
                b =>
                {
                    b.Navigation("UserRoles");
                });
            modelBuilder.Entity(
                "Domain.Entities.Brand",
                b =>
                {
                    b.Navigation("Products");
                });
            modelBuilder.Entity(
                "Domain.Entities.Contact",
                b =>
                {
                    b.Navigation("Replies");
                });
            modelBuilder.Entity(
                "Domain.Entities.EmployeeProfile",
                b =>
                {
                    b.Navigation("CommissionRecords");
                    b.Navigation("KPIs");
                });
            modelBuilder.Entity(
                "Domain.Entities.Input",
                b =>
                {
                    b.Navigation("InputInfos");
                });
            modelBuilder.Entity(
                "Domain.Entities.InputStatus",
                b =>
                {
                    b.Navigation("InputReceipts");
                });
            modelBuilder.Entity(
                "Domain.Entities.Lead",
                b =>
                {
                    b.Navigation("Activities");
                });
            modelBuilder.Entity(
                "Domain.Entities.NewsCategory",
                b =>
                {
                    b.Navigation("News");
                });
            modelBuilder.Entity(
                "Domain.Entities.Option",
                b =>
                {
                    b.Navigation("OptionValues");
                });
            modelBuilder.Entity(
                "Domain.Entities.OptionValue",
                b =>
                {
                    b.Navigation("VariantOptionValues");
                });
            modelBuilder.Entity(
                "Domain.Entities.Output",
                b =>
                {
                    b.Navigation("OutputInfos");
                    b.Navigation("Returns");
                });
            modelBuilder.Entity(
                "Domain.Entities.OutputInfo",
                b =>
                {
                    b.Navigation("Returns");
                });
            modelBuilder.Entity(
                "Domain.Entities.OutputStatus",
                b =>
                {
                    b.Navigation("OutputOrders");
                });
            modelBuilder.Entity(
                "Domain.Entities.PartnerType",
                b =>
                {
                    b.Navigation("Suppliers");
                });
            modelBuilder.Entity(
                "Domain.Entities.Permission",
                b =>
                {
                    b.Navigation("RolePermissions");
                });
            modelBuilder.Entity(
                "Domain.Entities.Product",
                b =>
                {
                    b.Navigation("CompatibleWith");
                    b.Navigation("ProductTechnologies");
                    b.Navigation("ProductVariants");
                    b.Navigation("SupportedBy");
                });
            modelBuilder.Entity(
                "Domain.Entities.ProductCategory",
                b =>
                {
                    b.Navigation("Products");
                    b.Navigation("SubCategories");
                });
            modelBuilder.Entity(
                "Domain.Entities.ProductStatus",
                b =>
                {
                    b.Navigation("Products");
                });
            modelBuilder.Entity(
                "Domain.Entities.ProductVariant",
                b =>
                {
                    b.Navigation("InputInfos");
                    b.Navigation("OutputInfos");
                    b.Navigation("ProductCollectionPhotos");
                    b.Navigation("VariantOptionValues");
                });
            modelBuilder.Entity(
                "Domain.Entities.Supplier",
                b =>
                {
                    b.Navigation("InputReceipts");
                    b.Navigation("SupplierContacts");
                });
            modelBuilder.Entity(
                "Domain.Entities.SupplierStatus",
                b =>
                {
                    b.Navigation("Suppliers");
                });
            modelBuilder.Entity(
                "Domain.Entities.Technology",
                b =>
                {
                    b.Navigation("Images");
                    b.Navigation("ProductTechnologies");
                });
            modelBuilder.Entity(
                "Domain.Entities.TechnologyCategory",
                b =>
                {
                    b.Navigation("Technologies");
                });
            modelBuilder.Entity(
                "Domain.Entities.Vehicle",
                b =>
                {
                    b.Navigation("Documents");
                    b.Navigation("MaintenanceHistories");
                });
            #pragma warning restore 612, 618
        }
    }
}
