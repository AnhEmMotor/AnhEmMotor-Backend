using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class SettingsSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var existingKeys = await context.Settings
            .Select(s => s.Key)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var defaults = new Dictionary<string, string>
        {
            { SettingKeys.InventoryAlertLevel, "5" },
            { SettingKeys.DepositRatio, "50" },
            { SettingKeys.OrderValueExceeds, "100000000" },
            { SettingKeys.DepositType, "percentage" },
            { SettingKeys.FixedDepositAmount, "2000000" }
        };
        var depositTypes = new[] { "Xe máy", "Phụ tùng & xe máy", "Chỉ có phụ tùng", "Chỉ có phụ kiện" };
        foreach (var type in depositTypes)
        {
            defaults[$"Deposit_{type}_Threshold"] = defaults[SettingKeys.OrderValueExceeds];
            defaults[$"Deposit_{type}_Ratio"] = defaults[SettingKeys.DepositRatio];
        }
        var toAdd = defaults
            .Where(kv => !existingKeys.Contains(kv.Key, StringComparer.OrdinalIgnoreCase))
            .Select(kv => new Setting { Key = kv.Key, Value = kv.Value })
            .ToList();
        if (toAdd.Count == 0)
        {
            return;
        }
        await context.Settings.AddRangeAsync(toAdd, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
