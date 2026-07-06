using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Seeders;

public static class WorkshopDataSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, IConfiguration configuration, CancellationToken cancellationToken)
    {
        // MaintenanceHistory and WorkshopPayment are seeded via SQL scripts (anhemmotor-utils)
        // to avoid IDENTITY_INSERT issues. No-op here — call EnsureMaintenanceHistoryTableExistsAsync in MigrationExtensions.
        await Task.CompletedTask;
    }
}
