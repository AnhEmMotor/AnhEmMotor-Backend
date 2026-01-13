using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

/// <summary>
/// Seeder để khởi tạo các trạng thái sản phẩm từ constants
/// </summary>
public static class ProductStatusSeeder
{
    /// <summary>
    /// Seed các trạng thái sản phẩm từ Domain.Constants.Product.ProductStatus
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task SeedAsync(
        ApplicationDBContext context,
        CancellationToken cancellationToken)
    {
        var allStatuses = Domain.Constants.ProductStatus.All;

        if (allStatuses.Count == 0)
        {
            return;
        }

        var existingStatuses = await context.Set<Domain.Entities.ProductStatus>()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var newStatuses = allStatuses
            .Except(existingStatuses.Select(s => s.Key), StringComparer.OrdinalIgnoreCase)
            .Select(key => new Domain.Entities.ProductStatus { Key = key })
            .ToList();

        if (newStatuses.Count != 0)
        {
            await context.Set<Domain.Entities.ProductStatus>()
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
            var hasReferences = await context.Set<Domain.Entities.Product>()
                .AnyAsync(p => p.StatusId != null && statusKeys.Contains(p.StatusId), cancellationToken)
                .ConfigureAwait(false);

            if (!hasReferences)
            {
                context.Set<Domain.Entities.ProductStatus>().RemoveRange(statusesToDelete);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
