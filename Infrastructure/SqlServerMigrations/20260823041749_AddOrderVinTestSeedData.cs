using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class AddOrderVinTestSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET NOCOUNT ON;
SET XACT_ABORT ON;
DECLARE @VariantId int = 160;
DECLARE @ColorId int = 341;
DECLARE @Marker nvarchar(100) = N'AEM-ORDER-VIN-TEST-20260823';
DECLARE @Now datetimeoffset = TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00');
DECLARE @Price decimal(18, 2);

IF NOT EXISTS (SELECT 1 FROM dbo.ProductVariantColor WHERE Id = @ColorId AND ProductVariantId = @VariantId)
    THROW 50001, 'Configured test color does not belong to configured product variant.', 1;
SELECT @Price = COALESCE(Price, 30000000) FROM dbo.ProductVariant WHERE Id = @VariantId AND DeletedAt IS NULL;
IF @Price IS NULL
    THROW 50002, 'Configured test product variant does not exist.', 1;

DECLARE @SeedOrders TABLE (SeedKey int NOT NULL PRIMARY KEY, StatusId nvarchar(50) NOT NULL, CustomerName nvarchar(200) NOT NULL, CustomerPhone nvarchar(50) NOT NULL, CreatedAt datetimeoffset NOT NULL);
INSERT INTO @SeedOrders (SeedKey, StatusId, CustomerName, CustomerPhone, CreatedAt)
VALUES
    (1, N'pending', N'Khách VIN test 01', N'0908000001', DATEADD(MINUTE, -8, @Now)),
    (2, N'pending', N'Khách VIN test 02', N'0908000002', DATEADD(MINUTE, -7, @Now)),
    (3, N'pending', N'Khách VIN test 03', N'0908000003', DATEADD(MINUTE, -6, @Now)),
    (4, N'pending', N'Khách VIN test 04', N'0908000004', DATEADD(MINUTE, -5, @Now)),
    (5, N'confirmed_cod', N'Khách VIN test 05', N'0908000005', DATEADD(MINUTE, -4, @Now)),
    (6, N'confirmed_cod', N'Khách VIN test 06', N'0908000006', DATEADD(MINUTE, -3, @Now)),
    (7, N'confirmed_cod', N'Khách VIN test 07', N'0908000007', DATEADD(MINUTE, -2, @Now)),
    (8, N'confirmed_cod', N'Khách VIN test 08', N'0908000008', DATEADD(MINUTE, -1, @Now));

DECLARE @InsertedOrders TABLE (SeedKey int NOT NULL PRIMARY KEY, OutputId int NOT NULL);
INSERT INTO dbo.Output (CustomerName, CustomerPhone, CustomerAddress, StatusId, PaymentMethod, PaymentStatus, DepositRatio, Notes, CreatedAt, UpdatedAt, LastStatusChangedAt)
SELECT s.CustomerName, s.CustomerPhone, N'Địa chỉ test dữ liệu VIN', s.StatusId, N'COD', N'Unpaid', 50, CONCAT(@Marker, N'-', RIGHT(CONCAT(N'0', s.SeedKey), 2)), s.CreatedAt, s.CreatedAt, s.CreatedAt
FROM @SeedOrders s
WHERE NOT EXISTS (SELECT 1 FROM dbo.Output e WHERE e.Notes = CONCAT(@Marker, N'-', RIGHT(CONCAT(N'0', s.SeedKey), 2)));
INSERT INTO @InsertedOrders (SeedKey, OutputId)
SELECT s.SeedKey, e.id FROM @SeedOrders s INNER JOIN dbo.Output e ON e.Notes = CONCAT(@Marker, N'-', RIGHT(CONCAT(N'0', s.SeedKey), 2));
INSERT INTO dbo.OutputInfo (ProductVariantId, ProductVariantColorId, Count, OutputId, Price, CostPrice, CreatedAt, UpdatedAt)
SELECT @VariantId, @ColorId, 1, i.OutputId, @Price, @Price * 0.8, s.CreatedAt, s.CreatedAt
FROM @InsertedOrders i INNER JOIN @SeedOrders s ON s.SeedKey = i.SeedKey
WHERE NOT EXISTS (SELECT 1 FROM dbo.OutputInfo e WHERE e.OutputId = i.OutputId AND e.ProductVariantId = @VariantId AND e.ProductVariantColorId = @ColorId);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    DECLARE @Marker nvarchar(100) = N'AEM-ORDER-VIN-TEST-20260823';
    DELETE FROM dbo.OutputInfo WHERE OutputId IN (SELECT id FROM dbo.Output WHERE Notes LIKE @Marker + N'-%');
    DELETE FROM dbo.Output WHERE Notes LIKE @Marker + N'-%';
    COMMIT TRANSACTION;
    ");
        }
    }
}
