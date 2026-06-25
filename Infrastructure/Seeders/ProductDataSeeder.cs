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
        var hondaBrand = await context.Brands
            .FirstOrDefaultAsync(b => string.Compare(b.Name, "Honda") == 0, cancellationToken)
            .ConfigureAwait(false);
        var yamahaBrand = await context.Brands
            .FirstOrDefaultAsync(b => string.Compare(b.Name, "Yamaha") == 0, cancellationToken)
            .ConfigureAwait(false);
        var suzukiBrand = await context.Brands
            .FirstOrDefaultAsync(b => string.Compare(b.Name, "Suzuki") == 0, cancellationToken)
            .ConfigureAwait(false);
        var productsToSeed = new List<Product>
        {
            new Product
            {
                Name = "Honda Vision 2024",
                ShortDescription = "Dòng xe tay ga quốc dân Honda Vision 2024",
                CategoryId = bikeCategory.Id,
                BrandId = hondaBrand?.Id,
                StatusId = Domain.Constants.Product.ProductStatus.ForSale,
                ProductVariants =
                    new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            VariantName = "Tiêu chuẩn",
                            UrlSlug = "honda-vision-2024-standard-red",
                            Price = 31100000m,
                            SKU = "HO-VIS-2024-RED",
                            ProductVariantColors =
                                new List<ProductVariantColor>
                                    {
                                        new ProductVariantColor
                                        {
                                            ColorName = "Đỏ",
                                            ColorCode = "#FF0000",
                                            CoverImageUrl = null
                                        }
                                    }
                        },
                        new ProductVariant
                        {
                            VariantName = "Đặc biệt",
                            UrlSlug = "honda-vision-2024-special-blue",
                            Price = 34500000m,
                            SKU = "HO-VIS-2024-BLUE",
                            ProductVariantColors =
                                new List<ProductVariantColor>
                                    {
                                        new ProductVariantColor
                                        {
                                            ColorName = "Xanh",
                                            ColorCode = "#0000FF",
                                            CoverImageUrl = null
                                        }
                                    }
                        }
                    }
            },
            new Product
            {
                Name = "Honda SH 150i 2024",
                ShortDescription = "Dòng xe tay ga cao cấp Honda SH 150i 2024",
                CategoryId = bikeCategory.Id,
                BrandId = hondaBrand?.Id,
                StatusId = Domain.Constants.Product.ProductStatus.ForSale,
                ProductVariants =
                    new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            VariantName = "Cao cấp",
                            UrlSlug = "honda-sh-150i-premium-white",
                            Price = 96000000m,
                            SKU = "HO-SH150-2024-WHITE",
                            ProductVariantColors =
                                new List<ProductVariantColor>
                                    {
                                        new ProductVariantColor
                                        {
                                            ColorName = "Trắng",
                                            ColorCode = "#FFFFFF",
                                            CoverImageUrl = null
                                        }
                                    }
                        },
                        new ProductVariant
                        {
                            VariantName = "Thể thao",
                            UrlSlug = "honda-sh-150i-sport-black",
                            Price = 102000000m,
                            SKU = "HO-SH150-2024-BLACK",
                            ProductVariantColors =
                                new List<ProductVariantColor>
                                    {
                                        new ProductVariantColor
                                        {
                                            ColorName = "Đen",
                                            ColorCode = "#000000",
                                            CoverImageUrl = null
                                        }
                                    }
                        }
                    }
            },
            new Product
            {
                Name = "Yamaha Exciter 155 VVA",
                ShortDescription = "Vua đường phố Exciter 155 VVA",
                CategoryId = bikeCategory.Id,
                BrandId = yamahaBrand?.Id,
                StatusId = Domain.Constants.Product.ProductStatus.ForSale,
                ProductVariants =
                    new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            VariantName = "Tiêu chuẩn",
                            UrlSlug = "yamaha-exciter-155-standard-white",
                            Price = 48000000m,
                            SKU = "YM-EX155-2024-WHITE",
                            ProductVariantColors =
                                new List<ProductVariantColor>
                                    {
                                        new ProductVariantColor
                                        {
                                            ColorName = "Trắng",
                                            ColorCode = "#FFFFFF",
                                            CoverImageUrl = null
                                        }
                                    }
                        },
                        new ProductVariant
                        {
                            VariantName = "Cao cấp",
                            UrlSlug = "yamaha-exciter-155-premium-black",
                            Price = 52000000m,
                            SKU = "YM-EX155-2024-BLACK",
                            ProductVariantColors =
                                new List<ProductVariantColor>
                                    {
                                        new ProductVariantColor
                                        {
                                            ColorName = "Đen",
                                            ColorCode = "#000000",
                                            CoverImageUrl = null
                                        }
                                    }
                        }
                    }
            },
            new Product
            {
                Name = "Suzuki Raider R150",
                ShortDescription = "Xe côn tay Suzuki Raider R150",
                CategoryId = bikeCategory.Id,
                BrandId = suzukiBrand?.Id,
                StatusId = Domain.Constants.Product.ProductStatus.ForSale,
                ProductVariants =
                    new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            VariantName = "Tiêu chuẩn",
                            UrlSlug = "suzuki-raider-150-standard-blue",
                            Price = 50000000m,
                            SKU = "SZ-RAI150-2024-BLUE",
                            ProductVariantColors =
                                new List<ProductVariantColor>
                                    {
                                        new ProductVariantColor
                                        {
                                            ColorName = "Xanh",
                                            ColorCode = "#0000FF",
                                            CoverImageUrl = null
                                        }
                                    }
                        }
                    }
            }
        };
        await context.Set<OptionValue>().Include(v => v.Option).ToListAsync(cancellationToken).ConfigureAwait(false);
        var colorOption = await context.Set<Option>()
            .FirstOrDefaultAsync(
                o => string.Compare(o.Name, "Color") == 0 || string.Compare(o.Name, "Màu sắc") == 0,
                cancellationToken)
            .ConfigureAwait(false);
        var typeOption = await context.Set<Option>()
            .FirstOrDefaultAsync(
                o => string.Compare(o.Name, "VehicleType") == 0 || string.Compare(o.Name, "Loại xe") == 0,
                cancellationToken)
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
                    variant.VariantName = pVariant.VariantName;
                    if (pVariant.ProductVariantColors != null && pVariant.ProductVariantColors.Any())
                    {
                        var color = variant.ProductVariantColors.FirstOrDefault();
                        if (color == null)
                        {
                            color = new ProductVariantColor();
                            variant.ProductVariantColors.Add(color);
                        }
                        var pColor = pVariant.ProductVariantColors.First();
                        color.ColorName = pColor.ColorName;
                        color.ColorCode = pColor.ColorCode;
                        color.CoverImageUrl = pColor.CoverImageUrl;
                    }
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
                                cancellationToken)
                            .ConfigureAwait(false);
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
                                cancellationToken)
                            .ConfigureAwait(false);
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

