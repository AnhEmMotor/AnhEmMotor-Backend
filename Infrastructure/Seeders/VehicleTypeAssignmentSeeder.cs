using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class VehicleTypeAssignmentSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var vehicleTypeOption = await context.Set<Option>()
            .FirstOrDefaultAsync(option => option.Name == "VehicleType" || option.Name == "Loại xe", cancellationToken)
            .ConfigureAwait(false);
        if (vehicleTypeOption == null)
            return;
        var typeValues = await context.Set<OptionValue>()
            .Where(value => value.OptionId == vehicleTypeOption.Id && value.DeletedAt == null)
            .ToDictionaryAsync(value => value.Name ?? string.Empty, cancellationToken)
            .ConfigureAwait(false);
        var bikeCategory = await context.ProductCategories
            .FirstOrDefaultAsync(category => category.Name == "Xe máy" && category.DeletedAt == null, cancellationToken)
            .ConfigureAwait(false);
        if (bikeCategory == null)
            return;
        var products = await context.Products
            .Where(product => product.CategoryId == bikeCategory.Id && product.DeletedAt == null)
            .Include(product => product.ProductVariants)
            .ThenInclude(variant => variant.VariantOptionValues)
            .ThenInclude(link => link.OptionValue)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var changed = false;
        foreach (var product in products)
        {
            var typeName = Classify(product.Name);
            if (typeName == null || !typeValues.TryGetValue(typeName, out var targetType))
                continue;
            foreach (var variant in product.ProductVariants.Where(variant => variant.DeletedAt == null))
            {
                var currentTypeLinks = variant.VariantOptionValues
                    .Where(link => link.OptionValue?.OptionId == vehicleTypeOption.Id)
                    .ToList();
                var targetLink = currentTypeLinks.FirstOrDefault(link => link.OptionValueId == targetType.Id);
                foreach (var obsoleteLink in currentTypeLinks.Where(link => link != targetLink))
                {
                    context.Set<VariantOptionValue>().Remove(obsoleteLink);
                    changed = true;
                }
                if (targetLink == null)
                {
                    variant.VariantOptionValues
                        .Add(
                            new VariantOptionValue { OptionValueId = targetType.Id, CreatedAt = DateTimeOffset.UtcNow });
                    changed = true;
                }
            }
        }
        if (changed)
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string? Classify(string? productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
            return null;
        if (ContainsAny(productName, "SH", "Air Blade"))
            return "Xe ga";
        if (ContainsAny(productName, "Wave", "Future"))
            return "Xe số";
        if (ContainsAny(productName, "Exciter", "Raider"))
            return "Xe côn tay";
        if (ContainsAny(productName, "Z900"))
            return "Moto phân khối lớn";
        return null;
    }

    private static bool ContainsAny(string value, params string[] terms) => terms.Any(
        term => value.Contains(term, StringComparison.OrdinalIgnoreCase));
}
