using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class PredefinedOptionSeeder
{
    private static readonly string[] ObsoleteColorKeys = ["Color", "Màu sắc"];

    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var obsoleteColorOptions = await context.Set<PredefinedOption>()
            .Where(p => ObsoleteColorKeys.Contains(p.Key) || ObsoleteColorKeys.Contains(p.Value))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (obsoleteColorOptions.Count > 0)
        {
            context.Set<PredefinedOption>().RemoveRange(obsoleteColorOptions);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var existingKeys = await context.Set<PredefinedOption>()
            .Select(p => p.Key)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var newOptions = PredefinedOptions.Options
            .Where(kv => !existingKeys.Contains(kv.Key, StringComparer.OrdinalIgnoreCase))
            .Select(kv => new PredefinedOption { Key = kv.Key, Value = kv.Value, })
            .ToList();
        if (newOptions.Count != 0)
        {
            await context.Set<PredefinedOption>().AddRangeAsync(newOptions, cancellationToken).ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
