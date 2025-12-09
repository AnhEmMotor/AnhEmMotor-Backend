using Domain.Entities;
using Infrastructure.DBContexts;
using Infrastructure.Seeders;
using Microsoft.AspNetCore.Identity;

namespace WebAPI.Extensions;

/// <summary>
/// Provides extension methods for applying database migrations and seeding initial data to an ASP.NET Core application
/// at startup.
/// </summary>
/// <remarks>These extensions are intended to be used during application startup to ensure the database schema is
/// up to date and required seed data is present. The seeding operation is controlled by the configuration setting
/// 'SeedingOptions:RunDataSeedingOnStartup'. If enabled, roles, users, permissions, and protected entities are seeded
/// using the application's registered services. This class is typically used in the Program.cs file as part of the
/// application's initialization pipeline.</remarks>
public static class MigrationExtensions
{
    /// <summary>
    /// Applies any pending database migrations and seeds initial data for the application if configured to do so.
    /// </summary>
    /// <remarks>This method should be called during application startup to ensure the database schema is up
    /// to date and required seed data is present. Data seeding is performed only if the configuration value
    /// 'SeedingOptions:RunDataSeedingOnStartup' is set to <see langword="true"/>. If an error occurs during migration
    /// or seeding, the exception is logged and rethrown, which may terminate the application.</remarks>
    /// <param name="app">The <see cref="WebApplication"/> instance whose services will be used to perform migrations and data seeding.
    /// Must not be null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task ApplyMigrationsAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var configuration = services.GetRequiredService<IConfiguration>();

        // Logger là cần thiết để debug khi deploy lên server
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var dbContext = services.GetRequiredService<ApplicationDBContext>();

            var shouldSeed = configuration.GetValue<bool>("SeedingOptions:RunDataSeedingOnStartup");
            if (shouldSeed)
            {
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var cancellationToken = app.Lifetime.ApplicationStopping;

                await PermissionDataSeeder.SeedPermissionsAsync(dbContext, cancellationToken);
                await ProtectedEntitiesSeeder.SeedProtectedEntitiesAsync(
                    dbContext,
                    roleManager,
                    userManager,
                    configuration,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during migration/seeding.");
            throw;
        }
    }
}
