using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

/// <summary>
/// Seeder để khởi tạo các trạng thái đơn hàng từ constants
/// </summary>
public static class OutputStatusSeeder
{
    /// <summary>
    /// Seed các trạng thái đơn hàng từ Domain.Constants.Order.OrderStatus
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var allStatuses = Domain.Constants.Order.OrderStatus.All;

        if(allStatuses.Count == 0)
        {
            return;
        }

        var existingStatuses = await context.Set<Domain.Entities.OutputStatus>()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var newStatuses = allStatuses
            .Except(existingStatuses.Select(s => s.Key), StringComparer.OrdinalIgnoreCase)
            .Select(key => new Domain.Entities.OutputStatus { Key = key })
            .ToList();

        if(newStatuses.Count != 0)
        {
            await context.Set<Domain.Entities.OutputStatus>()
                .AddRangeAsync(newStatuses, cancellationToken)
                .ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var statusesToDelete = existingStatuses
            .Where(s => !allStatuses.Contains(s.Key, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if(statusesToDelete.Count != 0)
        {
            var statusKeys = statusesToDelete.Select(s => s.Key).ToList();
            var hasReferences = await context.Set<Domain.Entities.Output>()
                .AnyAsync(o => o.StatusId != null && statusKeys.Contains(o.StatusId), cancellationToken)
                .ConfigureAwait(false);

            if(!hasReferences)
            {
                context.Set<Domain.Entities.OutputStatus>().RemoveRange(statusesToDelete);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
