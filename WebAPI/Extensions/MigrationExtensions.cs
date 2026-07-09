using Domain.Entities;
using Infrastructure.DBContexts;
using Infrastructure.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

		try
		{
			await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
			logger.LogInformation("EF Core MigrateAsync completed successfully.");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "FATAL: MigrateAsync failed — {Error}", ex.Message);
			Console.Error.WriteLine($"FATAL: MigrateAsync failed — {ex.Message}");
			throw;
		}

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
