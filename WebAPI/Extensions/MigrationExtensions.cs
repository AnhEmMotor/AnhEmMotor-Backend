using Domain.Entities;
using Infrastructure.DBContexts;
using Infrastructure.Seeders;
using Microsoft.AspNetCore.Identity;

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
    /// <see langword="true"/>. This method should be called during application startup to ensure the database schema is
    /// up to date and required data is present.
    /// </remarks>
    /// <param name="app">The current <see cref="WebApplication"/> instance to which migrations and seeding will be applied.</param>
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

            var shouldSeed = configuration.GetValue<bool>("SeedingOptions:RunDataSeedingOnStartup");
            if(shouldSeed)
            {
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                await ProductCategorySeeder.SeedAsync(dbContext, configuration, cancellationToken).ConfigureAwait(true);
                await InputStatusSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(true);
                await OutputStatusSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(true);
                await SupplierStatusSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(true);
                await ProductStatusSeeder.SeedAsync(dbContext, cancellationToken).ConfigureAwait(true);
                await PermissionDataSeeder.SeedPermissionsAsync(dbContext, cancellationToken).ConfigureAwait(true);
                await ProtectedEntitiesSeeder.SeedProtectedEntitiesAsync(
                    dbContext,
                    roleManager,
                    userManager,
                    configuration,
                    cancellationToken)
                    .ConfigureAwait(true);
            }
        } catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred during migration/seeding.");
            throw;
        }
    }
}
