using Domain.Entities;
using Infrastructure.DBContexts;
using Infrastructure.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Extensions;

/// <summary>
/// Provides extension methods for applying database migrations and seeding initial data to an ASP.NET Core application
/// at startup.
/// </summary>
/// <remarks>
/// These extensions are intended to be used during application startup to ensure the database schema is up to date and
/// required seed data is present. The seeding operation is controlled by the configuration setting
/// 'SeedingOptions:RunDataSeedingOnStartup'. If enabled, roles, users, permissions, and protected entities are seeded
/// using the application's registered services. This class is typically used in the Program.cs file as part of the
/// application's initialization pipeline.
/// </remarks>
public static class MigrationExtensions
{
    /// <summary>
    /// Applies any pending database migrations and optionally seeds initial data during application startup.
    /// </summary>
    /// <remarks>
    /// Data seeding is performed only if the configuration value 'SeedingOptions:RunDataSeedingOnStartup' is set to
    /// <see langword="true" />. This method should be called during application startup to ensure the database schema
    /// is up to date and required data is present.
    /// </remarks>
    /// <param name="app">The current <see cref="WebApplication" /> instance to which migrations and seeding will be applied.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the migration and seeding operations.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task ApplyMigrationsAndSeedAsync(this WebApplication app, CancellationToken cancellationToken)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        try
        {
            var dbContext = services.GetRequiredService<ApplicationDBContext>();
            await RepairMigrationDriftAsync(dbContext, logger, cancellationToken).ConfigureAwait(false);
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            var shouldSeed = configuration.GetValue<bool>("SeedingOptions:RunDataSeedingOnStartup");
            if (shouldSeed)
            {
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                await ProductCategorySeeder.SeedAsync(dbContext, configuration, cancellationToken).ConfigureAwait(false);
                await InventoryReceiptStatusSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await OutputStatusSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await SupplierStatusSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await PredefinedOptionSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await ProductOptionSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await ProductStatusSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await VehicleTypeAssignmentSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await SettingsSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await NewsCategorySeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await TechnologySeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await PermissionDataSeeder.SeedPermissionsAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await ProtectedEntitiesSeeder.SeedProtectedEntitiesAsync(
                    dbContext,
                    roleManager,
                    userManager,
                    configuration,
                    cancellationToken)
                    .ConfigureAwait(false);
                await EmployeeSeeder.SeedAsync(dbContext, userManager, cancellationToken).ConfigureAwait(false);
                await LeadSeeder.SeedAsync(dbContext, userManager, cancellationToken).ConfigureAwait(false);
                await CommissionPolicySeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await SupplierContractSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await FinanceContractSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await SalesAndInventorySeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await CarrierPartnerSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await LogisticsDataSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
                await WorkshopAndServiceSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
            }
        } catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during migration/seeding.");
            throw;
        }
    }

    /// <summary>
    /// Repairs migration drift by inserting missing migration records into __EFMigrationsHistory
    /// when the corresponding schema objects already exist in the database.
    /// </summary>
    /// <remarks>
    /// This handles the common local-dev scenario where the database has schema objects from
    /// migrations but the __EFMigrationsHistory table is missing the corresponding records —
    /// typically due to partial restores, manual schema changes, or migration history
    /// being cleared.
    ///
    /// Each entry in <see cref="_migrationSignatures"/> defines a migration ID and a SQL query
    /// that returns a non-null/non-zero value when that migration's effects are present.
    /// The check is idempotent: if the migration is already recorded, or the signature
    /// isn't found, no action is taken for that migration.
    ///
    /// To add a new migration signature, add a tuple to <see cref="_migrationSignatures"/>.
    /// </remarks>
    private static async Task RepairMigrationDriftAsync(
        ApplicationDBContext dbContext,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        const string historyTable = "__EFMigrationsHistory";

        // List of (migrationId, signatureCheckSql).
        // The SQL must return a scalar: null/DBNull = not present, non-zero/int = present.
        // Add entries for migrations whose effects may exist in the DB without a history record.
        var migrationSignatures = new (string MigrationId, string SignatureSql)[]
        {
            // InitialCreate — Banner table is a reliable signature
            ("20260509132251_InitialCreate",
             "SELECT OBJECT_ID('Banner', 'U')"),

            // AddSupplierTypeIdColumn — PartnerTypeId column on Supplier
            ("20260516011310_AddSupplierTypeIdColumn",
             "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Supplier' AND COLUMN_NAME = 'PartnerTypeId'"),
        };

        try
        {
            var connection = dbContext.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            foreach (var (migrationId, signatureSql) in migrationSignatures)
            {
                // Check if migration is already recorded
                using var historyCmd = connection.CreateCommand();
                historyCmd.CommandText = $"SELECT COUNT(*) FROM [{historyTable}] WHERE [MigrationId] = '{migrationId}'";
                var count = await historyCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

                if (count != null && Convert.ToInt32(count) > 0)
                {
                    continue; // Already recorded, no drift for this migration
                }

                // Check if migration's signature exists in the database
                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = signatureSql;
                var result = await checkCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

                if (result == DBNull.Value || result == null || (result is int intResult && intResult == 0))
                {
                    continue; // Signature not present — migration was genuinely never applied
                }

                // Drift detected: insert the missing migration record
                logger.LogWarning(
                    "Migration drift detected: signature for '{MigrationId}' exists but history record is missing. "
                    + "Inserting missing history row.",
                    migrationId);

                using var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = $"INSERT INTO [{historyTable}] ([MigrationId], [ProductVersion]) VALUES ('{migrationId}', '10.0.0')";
                await insertCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                logger.LogInformation("Successfully repaired migration drift for '{MigrationId}'.", migrationId);
            }
        }
catch (Exception ex)
  {
    if (ex is SqlException sqlEx && sqlEx.Number == 4060)
    {
      // Error 4060 = cannot open database — local DB does not exist at all.
      // MigrateAsync() will create it; no drift to repair, skip without stack trace noise.
      logger.LogInformation("Migration drift repair skipped: database does not exist yet (Error 4060).");
    }
    else
    {
      logger.LogWarning(ex, "Migration drift repair check failed (non-fatal): {Message}", ex.Message);
    }
  }
    }
}
