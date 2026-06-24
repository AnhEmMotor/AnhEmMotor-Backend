using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class ProductOptionSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var typeOption = await context.Set<Option>()
            .FirstOrDefaultAsync(
                o => string.Compare(o.Name, "VehicleType") == 0 || string.Compare(o.Name, "Loại xe") == 0,
                cancellationToken)
            .ConfigureAwait(false);
        if (typeOption == null)
        {
            typeOption = new Option { Name = "VehicleType" };
            await context.Set<Option>().AddAsync(typeOption, cancellationToken).ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        var legacyManualType = await context.Set<OptionValue>()
            .FirstOrDefaultAsync(
                v => v.OptionId == typeOption.Id && v.Name == "Tay côn",
                cancellationToken)
            .ConfigureAwait(false);
        var manualTypeExists = await context.Set<OptionValue>()
            .AnyAsync(
                v => v.OptionId == typeOption.Id && v.Name == "Xe côn tay",
                cancellationToken)
            .ConfigureAwait(false);
        if (legacyManualType != null && !manualTypeExists)
        {
            legacyManualType.Name = "Xe côn tay";
            legacyManualType.UpdatedAt = DateTimeOffset.UtcNow;
        }

        var types = new[] { "Xe ga", "Xe số", "Xe côn tay", "Moto phân khối lớn" };
        foreach (var type in types)
        {
            if (!await context.Set<OptionValue>()
                .AnyAsync(v => v.OptionId == typeOption.Id && string.Compare(v.Name, type) == 0, cancellationToken)
                .ConfigureAwait(false))
            {
                await context.Set<OptionValue>()
                    .AddAsync(new OptionValue { OptionId = typeOption.Id, Name = type }, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
