using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommissionPolicyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EffectiveDate",
                table: "CommissionPolicy",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(
                    new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    new TimeSpan(0, 0, 0, 0, 0)));
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CommissionPolicy",
                type: "bit",
                nullable: false,
                defaultValue: false);
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "CommissionPolicy",
                type: "nvarchar(500)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "EffectiveDate", table: "CommissionPolicy");
            migrationBuilder.DropColumn(name: "IsActive", table: "CommissionPolicy");
            migrationBuilder.DropColumn(name: "Notes", table: "CommissionPolicy");
        }
    }
}
