using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class PredefinedOptionSeeder
{
    private static readonly Dictionary<string, string> DefaultOptions = new()
    {
        { "VehicleType", "Loại xe" },
        { "Displacement", "Phân khối" },
        { "Condition", "Tình trạng" },
        { "Color", "Màu sắc" },
        { "ManufactureYear", "Năm sản xuất" },
        { "BrakeSystem", "Hệ thống phanh" },
        { "Size", "Kích cỡ" },
        { "Material", "Chất liệu" },
        { "Style", "Phong cách" },
    };

    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var existingKeys = await context.Set<Domain.Entities.PredefinedOption>()
            .Select(p => p.Key)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var newOptions = DefaultOptions
            .Where(kv => !existingKeys.Contains(kv.Key, StringComparer.OrdinalIgnoreCase))
            .Select(kv => new Domain.Entities.PredefinedOption { Key = kv.Key, Value = kv.Value, })
            .ToList();

        if(newOptions.Count != 0)
        {
            await context.Set<Domain.Entities.PredefinedOption>()
                .AddRangeAsync(newOptions, cancellationToken)
                .ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
