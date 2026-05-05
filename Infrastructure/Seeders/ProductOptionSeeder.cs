using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class ProductOptionSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var colorOption = await context.Set<Option>()
            .FirstOrDefaultAsync(o => string.Compare(o.Name, "Color") == 0 || string.Compare(o.Name, "Màu sắc") == 0, cancellationToken).ConfigureAwait(false);
        if (colorOption == null)
        {
            colorOption = new Option { Name = "Color" };
            await context.Set<Option>().AddAsync(colorOption, cancellationToken).ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        var colors = new Dictionary<string, string>
        {
            { "Trắng", "#FFFFFF" },
            { "Đen", "#000000" },
            { "Đỏ", "#FF0000" },
            { "Xanh", "#0000FF" },
            { "Bạc", "#C0C0C0" },
            { "Vàng", "#FFFF00" }
        };
        foreach (var color in colors)
        {
            var existing = await context.Set<OptionValue>()
                .FirstOrDefaultAsync(v => v.OptionId == colorOption.Id && string.Compare(v.Name, color.Key) == 0, cancellationToken).ConfigureAwait(false);
            if (existing == null)
            {
                await context.Set<OptionValue>()
                    .AddAsync(
                        new OptionValue { OptionId = colorOption.Id, Name = color.Key, ColorCode = color.Value },
                        cancellationToken).ConfigureAwait(false);
            } else
            {
                existing.ColorCode = color.Value;
            }
        }
        var typeOption = await context.Set<Option>()
            .FirstOrDefaultAsync(o => string.Compare(o.Name, "VehicleType") == 0 || string.Compare(o.Name, "Loại xe") == 0, cancellationToken).ConfigureAwait(false);
        if (typeOption == null)
        {
            typeOption = new Option { Name = "VehicleType" };
            await context.Set<Option>().AddAsync(typeOption, cancellationToken).ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        var types = new[] { "Xe ga", "Xe số", "Tay côn", "Xe điện" };
        foreach (var type in types)
        {
            if (!await context.Set<OptionValue>()
                .AnyAsync(v => v.OptionId == typeOption.Id && string.Compare(v.Name, type) == 0, cancellationToken).ConfigureAwait(false))
            {
                await context.Set<OptionValue>()
                    .AddAsync(new OptionValue { OptionId = typeOption.Id, Name = type }, cancellationToken).ConfigureAwait(false);
            }
        }
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
