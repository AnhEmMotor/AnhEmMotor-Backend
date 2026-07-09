using Domain.Entities;
using Infrastructure.DBContexts;
using Infrastructure.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
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
		var dbContext = services.GetRequiredService<ApplicationDBContext>();

		await dbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

		var applied = new HashSet<string>(StringComparer.Ordinal);
		await using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
		{
			cmd.CommandText = "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId";
			await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
			while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
				applied.Add(reader.GetString(0));
		}

		var allMigrations = new[]
		{
			"20260509132251_InitialCreate",
			"20260516011310_AddSupplierTypeIdColumn",
			"20260519141635_DropVehicleTypeAndUnusedProductColumns",
			"20260521085746_AddVehicleTrackingAndColorLinking",
			"20260522145111_FixProductVariantNamingAndAddVehicleColumns",
			"20260527081022_AddQuotationAndProductRows",
			"20260530013138_RefactorInputToInventoryReceiptAndPurchaseRequest",
			"20260610143232_MajorSchemaOverhaulInventoryQuotationsBannerAndNews",
			"20260613024229_AddBusinessContractsAndServiceManagementModules",
			"20260624123942_UpgradeInventoryServiceBookingAndAddCrmCmsModules",
			"20260625113447_RefactorServiceBookingAndAddSupplierDebt",
			"20260703140314_AddSalesAndWorkshopInvoicesAndWarranty",
			"20260704133950_AddPasswordResetTokenFields",
			"20260706073950_AddVouchers",
			"20260708081957_AddProductBrandLocalization",
			"20260708083146_AddJsonColumnsToProductAndBrand",
			"20260709000000_CreateShipmentsAndShipmentItems",
		};

		// Backfill any missing migration history records
		var missing = allMigrations.Where(m => !applied.Contains(m)).ToArray();
		if (missing.Length > 0)
		{
			logger.LogWarning("Migration drift: {Count} missing — backfilling history.", missing.Length);
			foreach (var id in missing)
			{
				await using var insert = dbContext.Database.GetDbConnection().CreateCommand();
				insert.CommandText = "INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES (@id, @ver)";
				var p1 = insert.CreateParameter(); p1.ParameterName = "@id"; p1.Value = id; insert.Parameters.Add(p1);
				var p2 = insert.CreateParameter(); p2.ParameterName = "@ver"; p2.Value = "10.0.9"; insert.Parameters.Add(p2);
				await insert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
			}
			logger.LogInformation("Backfilled {Count} migration history records.", missing.Length);
		}
		else
		{
			logger.LogInformation("All {Count} migrations are recorded. Skipping MigrateAsync.", applied.Count);
		}

		// Ensure any missing tables that are not covered by migrations are created
		await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
		logger.LogInformation("EnsureCreatedAsync completed.");

		var shouldSeed = configuration.GetValue<bool>("SeedingOptions:RunDataSeedingOnStartup");
		if (!shouldSeed) return;

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
			dbContext, roleManager, userManager, configuration, cancellationToken).ConfigureAwait(false);
		await EmployeeSeeder.SeedAsync(dbContext, userManager, cancellationToken).ConfigureAwait(false);
		await LeadSeeder.SeedAsync(dbContext, userManager, cancellationToken).ConfigureAwait(false);
		await CommissionPolicySeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
		await SupplierContractSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
		await FinanceContractSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
		await SalesAndInventorySeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
		await CarrierPartnerSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
		await LogisticsDataSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(false);
		await WorkshopDataSeeder.SeedAsync(dbContext, configuration, cancellationToken).ConfigureAwait(false);
	}
}
