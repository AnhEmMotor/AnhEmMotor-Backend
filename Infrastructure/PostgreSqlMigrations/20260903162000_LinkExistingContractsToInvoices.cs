using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
{
    /// <inheritdoc />
    public partial class LinkExistingContractsToInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
DECLARE
    rec RECORD;
    v_InvoiceId int;
    v_UserId uuid;
BEGIN
    SELECT ""Id"" INTO v_UserId FROM ""Users"" LIMIT 1;
    IF v_UserId IS NULL THEN
        v_UserId := gen_random_uuid();
    END IF;

    FOR rec IN 
        SELECT ""Id"", ""ContractNumber"", ""CustomerFullName"", ""CustomerCCCD"", ""CustomerPhone"", ""CustomerAddress"",
               ""VehicleModel"", ""VehicleColor"", ""FrameNumber"", ""EngineNumber"", ""ActualSalePrice"", ""CreatedAt""
        FROM ""SalesContracts""
        WHERE ""InvoiceId"" IS NULL
    LOOP
        SELECT ""Id"" INTO v_InvoiceId FROM ""Invoice"" WHERE ""ChassisNo"" = rec.""FrameNumber"" LIMIT 1;
        
        IF v_InvoiceId IS NULL THEN
            INSERT INTO ""Invoice"" (
                ""InvoiceNumber"", ""IssueDate"", ""TotalAmount"", ""Type"", ""UserId"",
                ""CustomerName"", ""CustomerIdCard"", ""CustomerPhone"", ""CustomerAddress"",
                ""VehicleModel"", ""VehicleColor"", ""ChassisNo"", ""EngineNo"",
                ""VehiclePrice"", ""RegistrationFee"", ""InsuranceFee"", ""PaymentMethod"",
                ""Status"", ""SalesPerson"", ""CreatedAt""
            ) VALUES (
                'INV-' || REPLACE(rec.""ContractNumber"", 'HD-BX-', 'BX-'),
                COALESCE(rec.""CreatedAt"", CURRENT_TIMESTAMP),
                rec.""ActualSalePrice"",
                'Sales',
                v_UserId,
                COALESCE(rec.""CustomerFullName"", 'Khách hàng'),
                COALESCE(rec.""CustomerCCCD"", '000000000000'),
                COALESCE(rec.""CustomerPhone"", '0900000000'),
                COALESCE(rec.""CustomerAddress"", 'TP. Hồ Chí Minh'),
                COALESCE(rec.""VehicleModel"", 'Xe máy'),
                COALESCE(rec.""VehicleColor"", 'Đen'),
                rec.""FrameNumber"",
                rec.""EngineNumber"",
                rec.""ActualSalePrice"",
                0,
                0,
                'transfer',
                'Completed',
                'Nhân viên Kinh doanh',
                COALESCE(rec.""CreatedAt"", CURRENT_TIMESTAMP)
            ) RETURNING ""Id"" INTO v_InvoiceId;
        END IF;

        UPDATE ""SalesContracts""
        SET ""InvoiceId"" = v_InvoiceId
        WHERE ""Id"" = rec.""Id"";
    END LOOP;
END $$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
