using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDBContext))]
    [Migration("20260612073500_RepairMissingProductHighlightsColumn")]
    public partial class RepairMissingProductHighlightsColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[Product]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'[dbo].[Product]', N'Highlights') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[Product] ADD [Highlights] nvarchar(MAX) NULL;
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
