using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc/>
    public partial class EditMediaFileTable : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(name: "PK_MediaFile", table: "MediaFile");

            migrationBuilder.DropColumn(name: "PublicUrl", table: "MediaFile");

            migrationBuilder.DropColumn(name: "StoredFileName", table: "MediaFile");

            migrationBuilder.RenameTable(name: "MediaFile", newName: "MediaFiles");

            migrationBuilder.AlterColumn<string>(
                name: "OriginalFileName",
                table: "MediaFiles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "MediaFiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "FileExtension",
                table: "MediaFiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoragePath",
                table: "MediaFiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageType",
                table: "MediaFiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddPrimaryKey(name: "PK_MediaFiles", table: "MediaFiles", column: "Id");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(name: "PK_MediaFiles", table: "MediaFiles");

            migrationBuilder.DropColumn(name: "FileExtension", table: "MediaFiles");

            migrationBuilder.DropColumn(name: "StoragePath", table: "MediaFiles");

            migrationBuilder.DropColumn(name: "StorageType", table: "MediaFiles");

            migrationBuilder.RenameTable(name: "MediaFiles", newName: "MediaFile");

            migrationBuilder.AlterColumn<string>(
                name: "OriginalFileName",
                table: "MediaFile",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "MediaFile",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicUrl",
                table: "MediaFile",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoredFileName",
                table: "MediaFile",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddPrimaryKey(name: "PK_MediaFile", table: "MediaFile", column: "Id");
        }
    }
}
