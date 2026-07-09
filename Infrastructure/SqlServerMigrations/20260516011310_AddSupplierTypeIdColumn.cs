using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.SqlServerMigrations
{
	/// <inheritdoc />
	public partial class AddSupplierTypeIdColumn : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
				IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'PartnerTypeId' AND object_id = OBJECT_ID('Supplier'))
				ALTER TABLE [Supplier] ADD [PartnerTypeId] nvarchar(50) NULL");

			migrationBuilder.Sql(@"
				IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PartnerType')
				BEGIN
				CREATE TABLE [PartnerType] (
					[Key] nvarchar(50) NOT NULL,
					[CreatedAt] datetimeoffset NULL,
					[UpdatedAt] datetimeoffset NULL,
					[DeletedAt] datetimeoffset NULL,
					CONSTRAINT [PK_PartnerType] PRIMARY KEY ([Key])
				);
				INSERT INTO [PartnerType] ([Key], [CreatedAt], [DeletedAt], [UpdatedAt]) VALUES
					('financial', NULL, NULL, NULL),
					('insurance', NULL, NULL, NULL),
					('supplier', NULL, NULL, NULL);
				END");

			migrationBuilder.Sql(@"
				IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Supplier_PartnerTypeId' AND object_id = OBJECT_ID('Supplier'))
				CREATE INDEX [IX_Supplier_PartnerTypeId] ON [Supplier] ([PartnerTypeId]);

				IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Supplier_PartnerType_PartnerTypeId')
				ALTER TABLE [Supplier] ADD CONSTRAINT [FK_Supplier_PartnerType_PartnerTypeId]
					FOREIGN KEY ([PartnerTypeId]) REFERENCES [PartnerType] ([Key]);
			");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(name: "FK_Supplier_PartnerType_PartnerTypeId", table: "Supplier");
			migrationBuilder.DropTable(name: "PartnerType");
			migrationBuilder.DropIndex(name: "IX_Supplier_PartnerTypeId", table: "Supplier");
			migrationBuilder.DropColumn(name: "PartnerTypeId", table: "Supplier");
		}
	}
}
