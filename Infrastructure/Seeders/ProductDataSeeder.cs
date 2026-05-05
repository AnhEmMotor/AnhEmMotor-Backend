using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class ProductDataSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var bikeCategory = await context.ProductCategories
            .FirstOrDefaultAsync(c => string.Compare(c.Name, "Xe máy") == 0, cancellationToken)
            .ConfigureAwait(false);
        if (bikeCategory == null)
            return;
        await context.Brands
            .FirstOrDefaultAsync(b => string.Compare(b.Name, "Honda") == 0, cancellationToken)
            .ConfigureAwait(false);
        await context.Brands
            .FirstOrDefaultAsync(b => string.Compare(b.Name, "Yamaha") == 0, cancellationToken)
            .ConfigureAwait(false);
        var productsToSeed = new List<Product>();
        await context.Set<OptionValue>()
            .Include(v => v.Option)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var colorOption = await context.Set<Option>()
            .FirstOrDefaultAsync(o => string.Compare(o.Name, "Color") == 0 || string.Compare(o.Name, "Màu sắc") == 0, cancellationToken)
            .ConfigureAwait(false);
        var typeOption = await context.Set<Option>()
            .FirstOrDefaultAsync(o => string.Compare(o.Name, "VehicleType") == 0 || string.Compare(o.Name, "Loại xe") == 0, cancellationToken)
            .ConfigureAwait(false);
        foreach (var p in productsToSeed)
        {
            var existing = await context.Products
                .Include(x => x.ProductVariants)
                .ThenInclude(v => v.VariantOptionValues)
                .FirstOrDefaultAsync(x => string.Compare(x.Name, p.Name) == 0, cancellationToken)
                .ConfigureAwait(false);
            if (existing == null)
            {
                await context.Products.AddAsync(p, cancellationToken).ConfigureAwait(false);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                existing = p;
            }
            foreach (var variant in existing.ProductVariants)
            {
                var pVariant = p.ProductVariants.FirstOrDefault(v => string.Compare(v.UrlSlug, variant.UrlSlug) == 0);
                if (pVariant != null)
                {
                    variant.VersionName = pVariant.VersionName;
                    variant.ColorName = pVariant.ColorName;
                    variant.ColorCode = pVariant.ColorCode;
                    variant.SKU = pVariant.SKU;
                    variant.Price = pVariant.Price;
                }
                if (variant.VariantOptionValues.Count == 0)
                {
                    var typeName = (p.Name ?? string.Empty).Contains("Vision") ||
                            (p.Name ?? string.Empty).Contains("SH")
                        ? "Xe ga"
                        : "Tay côn";
                    if (typeOption != null)
                    {
                        var typeVal = await context.Set<OptionValue>()
                            .FirstOrDefaultAsync(
                                v => v.OptionId == typeOption.Id && string.Compare(v.Name, typeName) == 0,
                                cancellationToken).ConfigureAwait(false);
                        if (typeVal != null)
                        {
                            variant.VariantOptionValues.Add(new VariantOptionValue { OptionValueId = typeVal.Id });
                        }
                    }
                    var colorName = (variant.UrlSlug ?? string.Empty).Contains("red")
                        ? "Đỏ"
                        : ((variant.UrlSlug ?? string.Empty).Contains("blue") ? "Xanh" : "Trắng");
                    if (colorOption != null)
                    {
                        var colorVal = await context.Set<OptionValue>()
                            .FirstOrDefaultAsync(
                                v => v.OptionId == colorOption.Id && string.Compare(v.Name, colorName) == 0,
                                cancellationToken).ConfigureAwait(false);
                        if (colorVal != null)
                        {
                            variant.VariantOptionValues.Add(new VariantOptionValue { OptionValueId = colorVal.Id });
                        }
                    }
                }
            }
            existing.BrandId = p.BrandId;
            existing.ShortDescription = p.ShortDescription;
            existing.Weight = p.Weight;
            existing.Dimensions = p.Dimensions;
            existing.Wheelbase = p.Wheelbase;
            existing.SeatHeight = p.SeatHeight;
            existing.GroundClearance = p.GroundClearance;
            existing.FuelCapacity = p.FuelCapacity;
            existing.TireSize = p.TireSize;
            existing.FrontSuspension = p.FrontSuspension;
            existing.RearSuspension = p.RearSuspension;
            existing.EngineType = p.EngineType;
            existing.MaxPower = p.MaxPower;
            existing.OilCapacity = p.OilCapacity;
            existing.FuelConsumption = p.FuelConsumption;
            existing.TransmissionType = p.TransmissionType;
            existing.StarterSystem = p.StarterSystem;
            existing.MaxTorque = p.MaxTorque;
            existing.Displacement = p.Displacement;
            existing.BoreStroke = p.BoreStroke;
            existing.CompressionRatio = p.CompressionRatio;
            existing.Description = p.Description;
        }
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
