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
			await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
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
		await WorkshopDataSeeder.SeedAsync(dbContext2, configuration, cancellationToken).ConfigureAwait(false);
	}
}
