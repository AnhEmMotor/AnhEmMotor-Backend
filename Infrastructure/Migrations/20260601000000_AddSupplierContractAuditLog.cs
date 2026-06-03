using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Infrastructure.DBContexts;

#nullable disable

namespace Infrastructure.Migrations;

[Migration("20260601000000_AddSupplierContractAuditLog")]
public partial class AddSupplierContractAuditLog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SupplierContractAuditLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SupplierContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                ChangedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                OldValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                NewValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SupplierContractAuditLogs", x => x.Id);
                table.ForeignKey(
                    name: "FK_SupplierContractAuditLogs_SupplierContracts_SupplierContractId",
                    column: x => x.SupplierContractId,
                    principalTable: "SupplierContracts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SupplierContractAuditLogs_SupplierContractId",
            table: "SupplierContractAuditLogs",
            column: "SupplierContractId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SupplierContractAuditLogs");
    }
}
