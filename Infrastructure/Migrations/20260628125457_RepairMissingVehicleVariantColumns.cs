using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RepairMissingVehicleVariantColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                // Add columns if they do not exist
                migrationBuilder.Sql(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicle') AND name = 'ProductVariantId')
                    BEGIN
                        ALTER TABLE Vehicle ADD ProductVariantId INT NULL;
                    END
                ");

                migrationBuilder.Sql(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicle') AND name = 'ProductVariantColorId')
                    BEGIN
                        ALTER TABLE Vehicle ADD ProductVariantColorId INT NULL;
                    END
                ");

                migrationBuilder.Sql(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicle') AND name = 'Status')
                    BEGIN
                        ALTER TABLE Vehicle ADD Status NVARCHAR(50) NOT NULL DEFAULT 'Available';
                    END
                ");

                // Add foreign keys if they do not exist
                migrationBuilder.Sql(@"
                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Vehicle_ProductVariant_ProductVariantId')
                    BEGIN
                        ALTER TABLE Vehicle ADD CONSTRAINT FK_Vehicle_ProductVariant_ProductVariantId
                        FOREIGN KEY (ProductVariantId) REFERENCES ProductVariant(Id);
                    END
                ");

                migrationBuilder.Sql(@"
                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Vehicle_ProductVariantColor_ProductVariantColorId')
                    BEGIN
                        ALTER TABLE Vehicle ADD CONSTRAINT FK_Vehicle_ProductVariantColor_ProductVariantColorId
                        FOREIGN KEY (ProductVariantColorId) REFERENCES ProductVariantColor(Id);
                    END
                ");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql(@"
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Vehicle_ProductVariantColor_ProductVariantColorId')
                    BEGIN
                        ALTER TABLE Vehicle DROP CONSTRAINT FK_Vehicle_ProductVariantColor_ProductVariantColorId;
                    END
                ");

                migrationBuilder.Sql(@"
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Vehicle_ProductVariant_ProductVariantId')
                    BEGIN
                        ALTER TABLE Vehicle DROP CONSTRAINT FK_Vehicle_ProductVariant_ProductVariantId;
                    END
                ");

                migrationBuilder.Sql(@"
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicle') AND name = 'Status')
                    BEGIN
                        ALTER TABLE Vehicle DROP COLUMN Status;
                    END
                ");

                migrationBuilder.Sql(@"
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicle') AND name = 'ProductVariantColorId')
                    BEGIN
                        ALTER TABLE Vehicle DROP COLUMN ProductVariantColorId;
                    END
                ");

                migrationBuilder.Sql(@"
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicle') AND name = 'ProductVariantId')
                    BEGIN
                        ALTER TABLE Vehicle DROP COLUMN ProductVariantId;
                    END
                ");
            }
        }
    }
}
