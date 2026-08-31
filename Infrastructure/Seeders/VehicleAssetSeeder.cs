using Domain.Constants.Order;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class VehicleAssetSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var variants = await context.ProductVariants
            .Include(variant => variant.Product)
            .Include(variant => variant.ProductVariantColors)
            .Where(variant =>
                variant.Product != null &&
                !string.IsNullOrWhiteSpace(variant.Product.Name) &&
                !string.IsNullOrWhiteSpace(variant.VariantName) &&
                variant.ProductVariantColors.Any(color => !string.IsNullOrWhiteSpace(color.ColorName)))
            .OrderBy(variant => variant.ProductId)
            .ThenBy(variant => variant.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (variants.Count == 0)
            return;

        var vehicles = await context.Vehicles
            .Where(vehicle => vehicle.LeadId.HasValue)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var variantsById = variants.ToDictionary(variant => variant.Id);
        var variantsByProductId = variants
            .GroupBy(variant => variant.ProductId)
            .ToDictionary(group => group.Key, group => group.ToList());
        var preferredVariant = variants.FirstOrDefault(variant =>
                string.Equals(
                    variant.Product!.Name,
                    "Honda Air Blade 125cc ABS",
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(variant.VariantName, "Tiêu chuẩn", StringComparison.OrdinalIgnoreCase)) ??
            variants.FirstOrDefault(variant =>
                variant.Product!.Name!.Contains("Honda Air Blade", StringComparison.OrdinalIgnoreCase)) ??
            variants[0];

        foreach (var vehicle in vehicles.Where(vehicle =>
                     !vehicle.ProductId.HasValue ||
                     !vehicle.ProductVariantId.HasValue ||
                     !vehicle.ProductVariantColorId.HasValue ||
                     string.Equals(vehicle.VinNumber, "VIN-NVA-12345", StringComparison.OrdinalIgnoreCase)))
        {
            var isLegacySeedVehicle = string.Equals(
                vehicle.VinNumber,
                "VIN-NVA-12345",
                StringComparison.OrdinalIgnoreCase);
            var variant = isLegacySeedVehicle
                ? preferredVariant
                : ResolveVariant(vehicle, variantsById, variantsByProductId, preferredVariant);
            var color = variant.ProductVariantColors
                .Where(item => !string.IsNullOrWhiteSpace(item.ColorName))
                .OrderByDescending(item =>
                    string.Equals(item.ColorName, "Đỏ đen", StringComparison.OrdinalIgnoreCase))
                .ThenBy(item => item.Id)
                .First();

            vehicle.ProductId = variant.ProductId;
            vehicle.ProductVariantId = variant.Id;
            vehicle.ProductVariantColorId = color.Id;
        }

        var leadIdsWithVehicles = vehicles
            .Where(vehicle => vehicle.LeadId.HasValue)
            .Select(vehicle => vehicle.LeadId!.Value)
            .ToHashSet();
        var leadsWithoutVehicles = await context.Leads
            .Where(lead => !leadIdsWithVehicles.Contains(lead.Id))
            .OrderBy(lead => lead.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var lead in leadsWithoutVehicles)
        {
            var variant = variants[lead.Id % variants.Count];
            var color = variant.ProductVariantColors
                .Where(item => !string.IsNullOrWhiteSpace(item.ColorName))
                .OrderByDescending(item =>
                    string.Equals(item.ColorName, "Đỏ đen", StringComparison.OrdinalIgnoreCase))
                .ThenBy(item => item.Id)
                .First();

            context.Vehicles.Add(new Vehicle
            {
                LeadId = lead.Id,
                ProductId = variant.ProductId,
                ProductVariantId = variant.Id,
                ProductVariantColorId = color.Id,
                VinNumber = $"AEM{lead.Id:D014}",
                EngineNumber = $"ENG-AEM-{lead.Id:D8}",
                LicensePlate = $"59A1-{10000 + lead.Id % 90000:D5}",
                PurchaseDate = DateTimeOffset.UtcNow.AddMonths(-(lead.Id % 18 + 1)),
                IsActive = true,
                Status = VehicleStatus.Available
            });
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static ProductVariant ResolveVariant(
        Vehicle vehicle,
        IReadOnlyDictionary<int, ProductVariant> variantsById,
        IReadOnlyDictionary<int, List<ProductVariant>> variantsByProductId,
        ProductVariant preferredVariant)
    {
        if (vehicle.ProductVariantId.HasValue &&
            variantsById.TryGetValue(vehicle.ProductVariantId.Value, out var assignedVariant))
        {
            return assignedVariant;
        }

        if (vehicle.ProductId.HasValue &&
            variantsByProductId.TryGetValue(vehicle.ProductId.Value, out var productVariants))
        {
            return productVariants[0];
        }

        return preferredVariant;
    }
}
