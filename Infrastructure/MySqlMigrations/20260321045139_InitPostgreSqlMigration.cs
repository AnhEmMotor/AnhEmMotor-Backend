using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class InitPostgreSqlMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "SupplierContact",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SupplierContact",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "SupplierContact",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Supplier",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Supplier",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Supplier",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Supplier",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "UrlSlug",
                table: "ProductVariant",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "CoverImageUrl",
                table: "ProductVariant",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "ProductCollectionPhoto",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Wheelbase",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                table: "Product",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "TransmissionType",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "TireSize",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "StarterSystem",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "ShortDescription",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<decimal>(
                name: "SeatHeight",
                table: "Product",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "RearSuspension",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<decimal>(
                name: "OilCapacity",
                table: "Product",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "MetaTitle",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "MetaDescription",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "MaxTorque",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "MaxPower",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<decimal>(
                name: "GroundClearance",
                table: "Product",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "FuelConsumption",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(35)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<decimal>(
                name: "FuelCapacity",
                table: "Product",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "FrontSuspension",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "EngineType",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<decimal>(
                name: "Displacement",
                table: "Product",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "Dimensions",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(35)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "CompressionRatio",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "BoreStroke",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "PredefinedOption",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)")
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "PredefinedOption",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)")
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "OptionValue",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Option",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Brand",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "SupplierContact",
                type: "nvarchar(15)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SupplierContact",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "SupplierContact",
                type: "nvarchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Supplier",
                type: "nvarchar(15)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Supplier",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Supplier",
                type: "nvarchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Supplier",
                type: "nvarchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "UrlSlug",
                table: "ProductVariant",
                type: "nvarchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "CoverImageUrl",
                table: "ProductVariant",
                type: "nvarchar(1000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "ProductCollectionPhoto",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Wheelbase",
                table: "Product",
                type: "nvarchar(20)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Weight",
                table: "Product",
                type: "nvarchar(20)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "TransmissionType",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "TireSize",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "StarterSystem",
                table: "Product",
                type: "nvarchar(30)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "ShortDescription",
                table: "Product",
                type: "nvarchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "SeatHeight",
                table: "Product",
                type: "nvarchar(20)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "RearSuspension",
                table: "Product",
                type: "nvarchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "OilCapacity",
                table: "Product",
                type: "nvarchar(250)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "MetaTitle",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "MetaDescription",
                table: "Product",
                type: "nvarchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "MaxTorque",
                table: "Product",
                type: "nvarchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "MaxPower",
                table: "Product",
                type: "nvarchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "GroundClearance",
                table: "Product",
                type: "nvarchar(20)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "FuelConsumption",
                table: "Product",
                type: "nvarchar(35)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "FuelCapacity",
                table: "Product",
                type: "nvarchar(20)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "FrontSuspension",
                table: "Product",
                type: "nvarchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "EngineType",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Displacement",
                table: "Product",
                type: "nvarchar(50)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "Dimensions",
                table: "Product",
                type: "nvarchar(35)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "CompressionRatio",
                table: "Product",
                type: "nvarchar(10)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "BoreStroke",
                table: "Product",
                type: "nvarchar(30)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "PredefinedOption",
                type: "nvarchar(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "PredefinedOption",
                type: "nvarchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "OptionValue",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Option",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Brand",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
