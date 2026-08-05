using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Infrastructure.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class UpdateNewsCommentPolymorphic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "NewsId",
                table: "NewsComments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.AddColumn<string>(
                name: "ArticleSlug",
                table: "NewsComments",
                type: "nvarchar(255)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ArticleType",
                table: "NewsComments",
                type: "nvarchar(50)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ArticleSlug", table: "NewsComments");
            migrationBuilder.DropColumn(name: "ArticleType", table: "NewsComments");
            migrationBuilder.AlterColumn<int>(
                name: "NewsId",
                table: "NewsComments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
