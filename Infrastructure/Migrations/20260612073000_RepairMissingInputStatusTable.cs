using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDBContext))]
    [Migration("20260612073000_RepairMissingInputStatusTable")]
    public partial class RepairMissingInputStatusTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[InputStatus]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[InputStatus] (
                        [Key] nvarchar(450) NOT NULL,
                        [CreatedAt] datetimeoffset NULL,
                        [UpdatedAt] datetimeoffset NULL,
                        [DeletedAt] datetimeoffset NULL,
                        CONSTRAINT [PK_InputStatus] PRIMARY KEY ([Key])
                    );
                END;

                IF OBJECT_ID(N'[dbo].[InputStatus]', N'U') IS NOT NULL
                BEGIN
                    INSERT INTO [dbo].[InputStatus] ([Key], [CreatedAt])
                    SELECT [Seed].[Key], SYSDATETIMEOFFSET()
                    FROM (VALUES (N'working'), (N'finished'), (N'cancelled')) AS [Seed]([Key])
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM [dbo].[InputStatus] AS [Existing]
                        WHERE [Existing].[Key] = [Seed].[Key]
                    );
                END;

                IF OBJECT_ID(N'[dbo].[Input]', N'U') IS NOT NULL
                   AND OBJECT_ID(N'[dbo].[InputStatus]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'[dbo].[Input]', N'StatusId') IS NOT NULL
                BEGIN
                    INSERT INTO [dbo].[InputStatus] ([Key], [CreatedAt])
                    SELECT DISTINCT [Input].[StatusId], SYSDATETIMEOFFSET()
                    FROM [dbo].[Input] AS [Input]
                    WHERE [Input].[StatusId] IS NOT NULL
                      AND NOT EXISTS (
                          SELECT 1
                          FROM [dbo].[InputStatus] AS [Existing]
                          WHERE [Existing].[Key] = [Input].[StatusId]
                      );
                END;

                IF OBJECT_ID(N'[dbo].[Input]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'[dbo].[Input]', N'StatusId') IS NOT NULL
                   AND NOT EXISTS (
                       SELECT 1
                       FROM sys.indexes
                       WHERE [name] = N'IX_Input_StatusId'
                         AND [object_id] = OBJECT_ID(N'[dbo].[Input]')
                   )
                BEGIN
                    CREATE INDEX [IX_Input_StatusId] ON [dbo].[Input] ([StatusId]);
                END;

                IF OBJECT_ID(N'[dbo].[Input]', N'U') IS NOT NULL
                   AND OBJECT_ID(N'[dbo].[InputStatus]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'[dbo].[Input]', N'StatusId') IS NOT NULL
                   AND NOT EXISTS (
                       SELECT 1
                       FROM sys.foreign_keys
                       WHERE [name] = N'FK_Input_InputStatus_StatusId'
                         AND [parent_object_id] = OBJECT_ID(N'[dbo].[Input]')
                   )
                BEGIN
                    ALTER TABLE [dbo].[Input] WITH CHECK ADD CONSTRAINT [FK_Input_InputStatus_StatusId]
                        FOREIGN KEY ([StatusId]) REFERENCES [dbo].[InputStatus] ([Key]);
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
