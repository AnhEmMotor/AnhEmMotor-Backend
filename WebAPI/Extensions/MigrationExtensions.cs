using Domain.Entities;
using Infrastructure.DBContexts;
using Infrastructure.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAndSeedAsync(this WebApplication app, CancellationToken cancellationToken)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var dbContext = services.GetRequiredService<ApplicationDBContext>();
            await ApplyMigrationsSafelyAsync(dbContext, logger, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration phase encountered errors. App will continue with seeding.");
        }

        var shouldSeed = configuration.GetValue<bool>("SeedingOptions:RunDataSeedingOnStartup");
        if (!shouldSeed) return;

        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext2 = services.GetRequiredService<ApplicationDBContext>();

        await ProductCategorySeeder.SeedAsync(dbContext2, configuration, cancellationToken).ConfigureAwait(false);
        await InventoryReceiptStatusSeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await OutputStatusSeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await SupplierStatusSeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await PredefinedOptionSeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await ProductOptionSeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await ProductStatusSeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await VehicleTypeAssignmentSeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await SettingsSeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await NewsCategorySeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await TechnologySeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await PermissionDataSeeder.SeedPermissionsAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await ProtectedEntitiesSeeder.SeedProtectedEntitiesAsync(dbContext2, roleManager, userManager, configuration, cancellationToken).ConfigureAwait(false);
        await EmployeeSeeder.SeedAsync(dbContext2, userManager, cancellationToken).ConfigureAwait(false);
        await LeadSeeder.SeedAsync(dbContext2, userManager, cancellationToken).ConfigureAwait(false);
        await CommissionPolicySeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await SupplierContractSeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await FinanceContractSeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await SalesAndInventorySeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await CarrierPartnerSeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await LogisticsDataSeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
        await WorkshopAndServiceSeeder.SeedAsync(dbContext2, cancellationToken).ConfigureAwait(false);
    }

    private static async Task ApplyMigrationsSafelyAsync(ApplicationDBContext dbContext, ILogger<Program> logger, CancellationToken cancellationToken)
    {
        var conn = dbContext.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        // Read already-applied migrations from __EFMigrationsHistory
        var applied = new HashSet<string>(StringComparer.Ordinal);
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT MigrationId FROM __EFMigrationsHistory";
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                applied.Add(reader.GetString(0));
        }

        // Also check actual DB schema: known tables/columns that cause false-positive migrations
        var existingObjects = await GetExistingSchemaObjectsAsync(conn, cancellationToken).ConfigureAwait(false);

        // Collect all known migrations from EF
        var allMigrations = dbContext.Database.GetMigrations();
        var lastApplied = applied.LastOrDefault() ?? string.Empty;
        var pending = allMigrations
            .Where(m => string.Compare(m, lastApplied, StringComparison.Ordinal) > 0)
            .ToList();

        foreach (var migrationId in pending)
        {
            // Pre-check: if all expected schema objects already exist, mark as applied silently
            if (await IsMigrationAlreadyInDatabaseAsync(migrationId, existingObjects, cancellationToken).ConfigureAwait(false))
            {
                logger.LogInformation("Migration {Migration} — schema already present, marking as applied.", migrationId);
                await MarkMigrationAsAppliedAsync(conn, migrationId, cancellationToken).ConfigureAwait(false);
                continue;
            }

            try
            {
                await dbContext.Database.MigrateAsync(migrationId, cancellationToken).ConfigureAwait(false);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (IsSchemaAlreadyExistsError(ex))
            {
                logger.LogWarning("Migration {Migration} skipped (err {ErrNo}) — schema object already exists. Marking as applied.", migrationId, ex.Number);
                await MarkMigrationAsAppliedAsync(conn, migrationId, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static async Task<HashSet<string>> GetExistingSchemaObjectsAsync(System.Data.Common.DbConnection conn, CancellationToken cancellationToken)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT TABLE_NAME + '.' + COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = 'dbo'
            UNION
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = 'dbo' AND TABLE_TYPE = 'BASE TABLE'";
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            set.Add(reader.GetString(0));
        return set;
    }

    private static async Task<bool> IsMigrationAlreadyInDatabaseAsync(
        string migrationId,
        HashSet<string> existingObjects,
        CancellationToken cancellationToken)
    {
        // Known schema objects introduced by each problematic migration
        // Check if the key objects from a migration already exist in DB
        return migrationId switch
        {
            "20260509132251_InitialCreate" => existingObjects.Contains("Banner"),
            "20260703140314_AddSalesAndWorkshopInvoicesAndWarranty"
                => existingObjects.Contains("Supplier.PartnerTypeId"),
            _ => false,
        };
    }

    private static async Task MarkMigrationAsAppliedAsync(
        System.Data.Common.DbConnection conn,
        string migrationId,
        CancellationToken cancellationToken)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = @id) " +
                          "INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES (@id, @ver)";
        var p1 = cmd.CreateParameter(); p1.ParameterName = "@id"; p1.Value = migrationId;
        var p2 = cmd.CreateParameter(); p2.ParameterName = "@ver"; p2.Value = "10.0.0";
        cmd.Parameters.Add(p1); cmd.Parameters.Add(p2);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static bool IsSchemaAlreadyExistsError(Microsoft.Data.SqlClient.SqlException ex)
    {
        return ex.Number is 2714 or 2705
            || ex.Message.Contains("already an object named", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("Column names in each table must be unique", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("is specified more than once", StringComparison.OrdinalIgnoreCase);
    }
}
