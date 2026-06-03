using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

public partial class AddContractTemplateVersioningFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Status",
            table: "ContractTemplates",
            type: "int",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.AddColumn<Guid>(
            name: "ParentId",
            table: "ContractTemplates",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsUsed",
            table: "ContractTemplates",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_ContractTemplates_ParentId",
            table: "ContractTemplates",
            column: "ParentId");

        migrationBuilder.AddForeignKey(
            name: "FK_ContractTemplates_ContractTemplates_ParentId",
            table: "ContractTemplates",
            column: "ParentId",
            principalTable: "ContractTemplates",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ContractTemplates_ContractTemplates_ParentId",
            table: "ContractTemplates");

        migrationBuilder.DropIndex(
            name: "IX_ContractTemplates_ParentId",
            table: "ContractTemplates");

        migrationBuilder.DropColumn(
            name: "Status",
            table: "ContractTemplates");

        migrationBuilder.DropColumn(
            name: "ParentId",
            table: "ContractTemplates");

        migrationBuilder.DropColumn(
            name: "IsUsed",
            table: "ContractTemplates");
    }
}
