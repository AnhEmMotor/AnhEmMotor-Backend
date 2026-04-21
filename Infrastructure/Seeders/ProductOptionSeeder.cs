using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class ProductOptionSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        // 1. Seed Colors
        var colorOption = await context.Set<Option>().FirstOrDefaultAsync(o => o.Name == "Color" || o.Name == "Màu sắc", cancellationToken);
        if (colorOption == null)
        {
            colorOption = new Option { Name = "Color" };
            await context.Set<Option>().AddAsync(colorOption, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        // DO NOT rename to "Màu sắc" because Name is a FK to PredefinedOption.Key

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
            var existing = await context.Set<OptionValue>().FirstOrDefaultAsync(v => v.OptionId == colorOption.Id && v.Name == color.Key, cancellationToken);
            if (existing == null)
            {
                await context.Set<OptionValue>().AddAsync(new OptionValue { OptionId = colorOption.Id, Name = color.Key, ColorCode = color.Value }, cancellationToken);
            }
            else
            {
                existing.ColorCode = color.Value;
            }
        }

        // 2. Seed Vehicle Types
        var typeOption = await context.Set<Option>().FirstOrDefaultAsync(o => o.Name == "VehicleType" || o.Name == "Loại xe", cancellationToken);
        if (typeOption == null)
        {
            typeOption = new Option { Name = "VehicleType" };
            await context.Set<Option>().AddAsync(typeOption, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        // DO NOT rename to "Loại xe" because Name is a FK to PredefinedOption.Key

        var types = new[] { "Xe ga", "Xe số", "Tay côn", "Xe điện" };
        foreach (var type in types)
        {
            if (!await context.Set<OptionValue>().AnyAsync(v => v.OptionId == typeOption.Id && v.Name == type, cancellationToken))
            {
                await context.Set<OptionValue>().AddAsync(new OptionValue { OptionId = typeOption.Id, Name = type }, cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
