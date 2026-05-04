using Domain.Constants.Order;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class OutputStatusSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var allStatuses = OrderStatus.All;
        if (allStatuses.Count == 0)
        {
            return;
        }
        var existingStatuses = await context.Set<OutputStatus>().ToListAsync(cancellationToken).ConfigureAwait(false);
        var newStatuses = allStatuses
            .Except(existingStatuses.Select(s => s.Key), StringComparer.OrdinalIgnoreCase)
            .Select(key => new OutputStatus { Key = key })
            .ToList();
        if (newStatuses.Count != 0)
        {
            await context.Set<OutputStatus>().AddRangeAsync(newStatuses, cancellationToken).ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        var statusesToDelete = existingStatuses
            .Where(s => !allStatuses.Contains(s.Key, StringComparer.OrdinalIgnoreCase))
            .ToList();
        if (statusesToDelete.Count != 0)
        {
            var statusKeys = statusesToDelete.Select(s => s.Key).ToList();
            var hasReferences = await context.Set<Output>()
                .AnyAsync(o => o.StatusId != null && statusKeys.Contains(o.StatusId), cancellationToken)
                .ConfigureAwait(false);
            if (!hasReferences)
            {
                context.Set<OutputStatus>().RemoveRange(statusesToDelete);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
