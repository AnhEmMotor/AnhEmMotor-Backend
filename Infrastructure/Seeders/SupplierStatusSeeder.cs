using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

/// <summary>
/// Seeder để khởi tạo các trạng thái nhà cung cấp từ constants
/// </summary>
public static class SupplierStatusSeeder
{
    /// <summary>
    /// Seed các trạng thái nhà cung cấp từ Domain.Constants.Supplier.SupplierStatus
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task SeedAsync(
        ApplicationDBContext context,
        CancellationToken cancellationToken)
    {
        var allStatuses = Domain.Constants.SupplierStatus.All;

        if (allStatuses.Count == 0)
        {
            return;
        }

        var existingStatuses = await context.Set<Domain.Entities.SupplierStatus>()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var newStatuses = allStatuses
            .Except(existingStatuses.Select(s => s.Key), StringComparer.OrdinalIgnoreCase)
            .Select(key => new Domain.Entities.SupplierStatus { Key = key })
            .ToList();

        if (newStatuses.Count != 0)
        {
            await context.Set<Domain.Entities.SupplierStatus>()
                .AddRangeAsync(newStatuses, cancellationToken)
                .ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var statusesToDelete = existingStatuses
            .Where(s => !allStatuses.Contains(s.Key, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (statusesToDelete.Count != 0)
        {
            var statusKeys = statusesToDelete.Select(s => s.Key).ToList();
            var hasReferences = await context.Set<Domain.Entities.Supplier>()
                .AnyAsync(s => s.StatusId != null && statusKeys.Contains(s.StatusId), cancellationToken)
                .ConfigureAwait(false);

            if (!hasReferences)
            {
                context.Set<Domain.Entities.SupplierStatus>().RemoveRange(statusesToDelete);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
