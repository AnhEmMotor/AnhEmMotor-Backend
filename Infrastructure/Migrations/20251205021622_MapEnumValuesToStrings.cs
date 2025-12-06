using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc/>
    public partial class MapEnumValuesToStrings : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                UPDATE [Users]
                SET [Gender] = CASE 
                    WHEN CAST([Gender] AS INT) = 0 THEN 'Male'
                    WHEN CAST([Gender] AS INT) = 1 THEN 'Female'
                    WHEN CAST([Gender] AS INT) = 2 THEN 'Other'
                    ELSE 'Male'
                END
            ");

            migrationBuilder.Sql(
                @"
                UPDATE [Users]
                SET [Status] = CASE 
                    WHEN CAST([Status] AS INT) = 0 THEN 'Inactive'
                    WHEN CAST([Status] AS INT) = 1 THEN 'Active'
                    WHEN CAST([Status] AS INT) = 2 THEN 'Banned'
                    WHEN CAST([Status] AS INT) = 3 THEN 'Suspended'
                    ELSE 'Active'
                END
            ");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                UPDATE [Users]
                SET [Gender] = CASE 
                    WHEN [Gender] = 'Male' THEN '0'
                    WHEN [Gender] = 'Female' THEN '1'
                    WHEN [Gender] = 'Other' THEN '2'
                    ELSE '0'
                END
            ");

            migrationBuilder.Sql(
                @"
                UPDATE [Users]
                SET [Status] = CASE 
                    WHEN [Status] = 'Inactive' THEN '0'
                    WHEN [Status] = 'Active' THEN '1'
                    WHEN [Status] = 'Banned' THEN '2'
                    WHEN [Status] = 'Suspended' THEN '3'
                    ELSE '1'
                END
            ");
        }
    }
}
