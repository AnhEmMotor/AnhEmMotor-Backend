using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOptionValueSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "OptionValue",
                type: "nvarchar(MAX)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "OptionValue",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "OptionValue",
                type: "bit",
                nullable: false,
                defaultValue: false);
            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                table: "OptionValue",
                type: "nvarchar(500)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                table: "OptionValue",
                type: "nvarchar(200)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Description", table: "OptionValue");
            migrationBuilder.DropColumn(name: "ImageUrl", table: "OptionValue");
            migrationBuilder.DropColumn(name: "IsActive", table: "OptionValue");
            migrationBuilder.DropColumn(name: "SeoDescription", table: "OptionValue");
            migrationBuilder.DropColumn(name: "SeoTitle", table: "OptionValue");
        }
    }
}
