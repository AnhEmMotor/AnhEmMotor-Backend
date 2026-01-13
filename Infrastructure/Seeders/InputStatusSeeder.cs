using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

/// <summary>
/// Seeder để khởi tạo các trạng thái đầu vào từ constants
/// </summary>
public static class InputStatusSeeder
{
    /// <summary>
    /// Seed các trạng thái đầu vào từ Domain.Constants.InputStatus
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var allStatuses = Domain.Constants.Input.InputStatus.AllowedValues;

        if(allStatuses.Length == 0)
        {
            return;
        }

        var existingStatuses = await context.Set<Domain.Entities.InputStatus>()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var newStatuses = allStatuses
            .Except(existingStatuses.Select(s => s.Key), StringComparer.OrdinalIgnoreCase)
            .Select(key => new Domain.Entities.InputStatus { Key = key })
            .ToList();

        if(newStatuses.Count != 0)
        {
            await context.Set<Domain.Entities.InputStatus>()
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
            var hasReferences = await context.Set<Domain.Entities.Input>()
                .AnyAsync(i => i.StatusId != null && statusKeys.Contains(i.StatusId), cancellationToken)
                .ConfigureAwait(false);

            if(!hasReferences)
            {
                context.Set<Domain.Entities.InputStatus>().RemoveRange(statusesToDelete);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
