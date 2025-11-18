using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDBContext))]
    partial class ApplicationDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Domain.Entities.Brand", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(MAX)")
                        .HasColumnName("Description");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("Name");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("Brand");
                });

            modelBuilder.Entity("Domain.Entities.Input", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("InputDate")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("InputDate");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(MAX)")
                        .HasColumnName("Notes");

                    b.Property<string>("StatusId")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("StatusId");

                    b.Property<int?>("SupplierId")
                        .HasColumnType("int")
                        .HasColumnName("SupplierId");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("StatusId");

                    b.HasIndex("SupplierId");

                    b.ToTable("Input");
                });

            modelBuilder.Entity("Domain.Entities.InputInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<short?>("Count")
                        .HasColumnType("smallint")
                        .HasColumnName("Count");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("InputId")
                        .HasColumnType("int")
                        .HasColumnName("InputId");

                    b.Property<long?>("InputPrice")
                        .HasColumnType("bigint")
                        .HasColumnName("InputPrice");

                    b.Property<int?>("ProductId")
                        .HasColumnType("int")
                        .HasColumnName("ProductId");

                    b.Property<long?>("RemainingCount")
                        .HasColumnType("bigint")
                        .HasColumnName("RemainingCount");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("InputId");

                    b.HasIndex("ProductId");

                    b.ToTable("InputInfo");
                });

            modelBuilder.Entity("Domain.Entities.InputStatus", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("Key");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Key");

                    b.ToTable("InputStatus");
                });

            modelBuilder.Entity("Domain.Entities.MediaFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<long?>("FileSize")
                        .HasColumnType("bigint");

                    b.Property<string>("OriginalFileName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("PublicUrl")
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("StoredFileName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("MediaFile");
                });

            modelBuilder.Entity("Domain.Entities.Option", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("Name");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("Option");
                });

            modelBuilder.Entity("Domain.Entities.OptionValue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("Name");

                    b.Property<int?>("OptionId")
                        .HasColumnType("int")
                        .HasColumnName("OptionId");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("OptionId");

                    b.ToTable("OptionValue");
                });

            modelBuilder.Entity("Domain.Entities.Output", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("CustomerName")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("CustomerName");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("EmpCode")
                        .HasColumnType("int")
                        .HasColumnName("EmpCode");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(MAX)")
                        .HasColumnName("Notes");

                    b.Property<string>("StatusId")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("StatusId");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("StatusId");

                    b.ToTable("Output");
                });

            modelBuilder.Entity("Domain.Entities.OutputInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long?>("CostPrice")
                        .HasColumnType("bigint")
                        .HasColumnName("CostPrice");

                    b.Property<short?>("Count")
                        .HasColumnType("smallint")
                        .HasColumnName("Count");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("OutputId")
                        .HasColumnType("int")
                        .HasColumnName("OutputId");

                    b.Property<long?>("Price")
                        .HasColumnType("bigint")
                        .HasColumnName("Price");

                    b.Property<int?>("ProductId")
                        .HasColumnType("int")
                        .HasColumnName("ProductId");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("OutputId");

                    b.HasIndex("ProductId");

                    b.ToTable("OutputInfo");
                });

            modelBuilder.Entity("Domain.Entities.OutputStatus", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("Key");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Key");

                    b.ToTable("OutputStatus");
                });

            modelBuilder.Entity("Domain.Entities.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("BoreStroke")
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("BoreStroke");

                    b.Property<int?>("BrandId")
                        .HasColumnType("int")
                        .HasColumnName("BrandId");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int")
                        .HasColumnName("CategoryId");

                    b.Property<string>("CompressionRatio")
                        .HasColumnType("nvarchar(10)")
                        .HasColumnName("CompressionRatio");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(MAX)")
                        .HasColumnName("Description");

                    b.Property<string>("Dimensions")
                        .HasColumnType("nvarchar(35)")
                        .HasColumnName("Dimensions");

                    b.Property<string>("Displacement")
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("Displacement");

                    b.Property<string>("EngineType")
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("EngineType");

                    b.Property<string>("FrontSuspension")
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("FrontSuspension");

                    b.Property<string>("FuelCapacity")
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("FuelCapacity");

                    b.Property<string>("FuelConsumption")
                        .HasColumnType("nvarchar(35)")
                        .HasColumnName("FuelConsumption");

                    b.Property<string>("GroundClearance")
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("GroundClearance");

                    b.Property<string>("MaxPower")
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("MaxPower");

                    b.Property<string>("MaxTorque")
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("MaxTorque");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("Name");

                    b.Property<string>("OilCapacity")
                        .HasColumnType("nvarchar(250)")
                        .HasColumnName("OilCapacity");

                    b.Property<string>("RearSuspension")
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("RearSuspension");

                    b.Property<string>("SeatHeight")
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("SeatHeight");

                    b.Property<string>("StarterSystem")
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("StarterSystem");

                    b.Property<string>("StatusId")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("StatusId");

                    b.Property<string>("TireSize")
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("TireSize");

                    b.Property<string>("TransmissionType")
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("TransmissionType");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Weight")
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("Weight");

                    b.Property<string>("Wheelbase")
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("Wheelbase");

                    b.HasKey("Id");

                    b.HasIndex("BrandId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("StatusId");

                    b.ToTable("Product");
                });

            modelBuilder.Entity("Domain.Entities.ProductCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Description");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Name");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("ProductCategory");
                });

            modelBuilder.Entity("Domain.Entities.ProductCollectionPhoto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("ImageUrl");

                    b.Property<int?>("ProductVariantId")
                        .HasColumnType("int")
                        .HasColumnName("ProductVariantId");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("ProductVariantId");

                    b.ToTable("ProductCollectionPhoto");
                });

            modelBuilder.Entity("Domain.Entities.ProductStatus", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("Key");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Key");

                    b.ToTable("ProductStatus");
                });

            modelBuilder.Entity("Domain.Entities.ProductVariant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CoverImageUrl")
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("CoverImageUrl");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<long?>("Price")
                        .HasColumnType("bigint")
                        .HasColumnName("Price");

                    b.Property<int?>("ProductId")
                        .HasColumnType("int")
                        .HasColumnName("ProductId");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("UrlSlug")
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("UrlSlug");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.ToTable("ProductVariant");
                });

            modelBuilder.Entity("Domain.Entities.Setting", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("Key");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<long?>("Value")
                        .HasColumnType("bigint")
                        .HasColumnName("Value");

                    b.HasKey("Key");

                    b.ToTable("Setting");
                });

            modelBuilder.Entity("Domain.Entities.Supplier", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("Id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("Address");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("Email");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("Name");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(MAX)")
                        .HasColumnName("Notes");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(15)")
                        .HasColumnName("Phone");

                    b.Property<string>("StatusId")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("StatusId");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("StatusId");

                    b.ToTable("Supplier");
                });

            modelBuilder.Entity("Domain.Entities.SupplierStatus", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("Key");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Key");

                    b.ToTable("SupplierStatus");
                });

            modelBuilder.Entity("Domain.Entities.VariantOptionValue", b =>
                {
                    b.Property<int>("VariantId")
                        .HasColumnType("int")
                        .HasColumnName("VariantId");

                    b.Property<int>("OptionValueId")
                        .HasColumnType("int")
                        .HasColumnName("OptionValueId");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("VariantId", "OptionValueId");

                    b.HasIndex("OptionValueId");

                    b.ToTable("VariantOptionValue");
                });

            modelBuilder.Entity("Domain.Entities.Input", b =>
                {
                    b.HasOne("Domain.Entities.InputStatus", "InputStatus")
                        .WithMany("InputReceipts")
                        .HasForeignKey("StatusId");

                    b.HasOne("Domain.Entities.Supplier", "Supplier")
                        .WithMany("InputReceipts")
                        .HasForeignKey("SupplierId");

                    b.Navigation("InputStatus");

                    b.Navigation("Supplier");
                });

            modelBuilder.Entity("Domain.Entities.InputInfo", b =>
                {
                    b.HasOne("Domain.Entities.Input", "InputReceipt")
                        .WithMany("InputInfos")
                        .HasForeignKey("InputId");

                    b.HasOne("Domain.Entities.ProductVariant", "ProductVariant")
                        .WithMany("InputInfos")
                        .HasForeignKey("ProductId");

                    b.Navigation("InputReceipt");

                    b.Navigation("ProductVariant");
                });

            modelBuilder.Entity("Domain.Entities.OptionValue", b =>
                {
                    b.HasOne("Domain.Entities.Option", "Option")
                        .WithMany("OptionValues")
                        .HasForeignKey("OptionId");

                    b.Navigation("Option");
                });

            modelBuilder.Entity("Domain.Entities.Output", b =>
                {
                    b.HasOne("Domain.Entities.OutputStatus", "OutputStatus")
                        .WithMany("OutputOrders")
                        .HasForeignKey("StatusId");

                    b.Navigation("OutputStatus");
                });

            modelBuilder.Entity("Domain.Entities.OutputInfo", b =>
                {
                    b.HasOne("Domain.Entities.Output", "OutputOrder")
                        .WithMany("OutputInfos")
                        .HasForeignKey("OutputId");

                    b.HasOne("Domain.Entities.ProductVariant", "ProductVariant")
                        .WithMany("OutputInfos")
                        .HasForeignKey("ProductId");

                    b.Navigation("OutputOrder");

                    b.Navigation("ProductVariant");
                });

            modelBuilder.Entity("Domain.Entities.Product", b =>
                {
                    b.HasOne("Domain.Entities.Brand", "Brand")
                        .WithMany("Products")
                        .HasForeignKey("BrandId");

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

            modelBuilder.Entity("Domain.Entities.ProductCollectionPhoto", b =>
                {
                    b.HasOne("Domain.Entities.ProductVariant", "ProductVariant")
                        .WithMany("ProductCollectionPhotos")
                        .HasForeignKey("ProductVariantId");

                    b.Navigation("ProductVariant");
                });

            modelBuilder.Entity("Domain.Entities.ProductVariant", b =>
                {
                    b.HasOne("Domain.Entities.Product", "Product")
                        .WithMany("ProductVariants")
                        .HasForeignKey("ProductId");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("Domain.Entities.Supplier", b =>
                {
                    b.HasOne("Domain.Entities.SupplierStatus", "SupplierStatus")
                        .WithMany("Suppliers")
                        .HasForeignKey("StatusId");

                    b.Navigation("SupplierStatus");
                });

            modelBuilder.Entity("Domain.Entities.VariantOptionValue", b =>
                {
                    b.HasOne("Domain.Entities.OptionValue", "OptionValue")
                        .WithMany("VariantOptionValues")
                        .HasForeignKey("OptionValueId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.ProductVariant", "ProductVariant")
                        .WithMany("VariantOptionValues")
                        .HasForeignKey("VariantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OptionValue");

                    b.Navigation("ProductVariant");
                });

            modelBuilder.Entity("Domain.Entities.Brand", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("Domain.Entities.Input", b =>
                {
                    b.Navigation("InputInfos");
                });

            modelBuilder.Entity("Domain.Entities.InputStatus", b =>
                {
                    b.Navigation("InputReceipts");
                });

            modelBuilder.Entity("Domain.Entities.Option", b =>
                {
                    b.Navigation("OptionValues");
                });

            modelBuilder.Entity("Domain.Entities.OptionValue", b =>
                {
                    b.Navigation("VariantOptionValues");
                });

            modelBuilder.Entity("Domain.Entities.Output", b =>
                {
                    b.Navigation("OutputInfos");
                });

            modelBuilder.Entity("Domain.Entities.OutputStatus", b =>
                {
                    b.Navigation("OutputOrders");
                });

            modelBuilder.Entity("Domain.Entities.Product", b =>
                {
                    b.Navigation("ProductVariants");
                });

            modelBuilder.Entity("Domain.Entities.ProductCategory", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("Domain.Entities.ProductStatus", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("Domain.Entities.ProductVariant", b =>
                {
                    b.Navigation("InputInfos");

                    b.Navigation("OutputInfos");

                    b.Navigation("ProductCollectionPhotos");

                    b.Navigation("VariantOptionValues");
                });

            modelBuilder.Entity("Domain.Entities.Supplier", b =>
                {
                    b.Navigation("InputReceipts");
                });

            modelBuilder.Entity("Domain.Entities.SupplierStatus", b =>
                {
                    b.Navigation("Suppliers");
                });
#pragma warning restore 612, 618
        }
    }
}
