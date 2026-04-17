using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Domain.Constants.Product;

namespace Infrastructure.Seeders;

public static class ProductDataSeeder
{
    public static async Task SeedAsync(
        ApplicationDBContext context,
        CancellationToken cancellationToken)
    {
        var bikeCategory = await context.ProductCategories
            .FirstOrDefaultAsync(c => c.Name == "Xe máy", cancellationToken)
            .ConfigureAwait(false);

        if (bikeCategory == null) return;

        var honda = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Honda", cancellationToken).ConfigureAwait(false);
        var yamaha = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Yamaha", cancellationToken).ConfigureAwait(false);

        var productsToSeed = new List<Product>
        {
            new Product
            {
                Name = "Honda SH 160i 2024",
                ShortDescription = "Đẳng cấp xe ga hạng sang, động cơ mạnh mẽ.",
                CategoryId = bikeCategory.Id,
                BrandId = honda?.Id,
                StatusId = Domain.Constants.Product.ProductStatus.ForSale,
                EngineType = "eSP+, 4 van, 4 kỳ, làm mát bằng dung dịch",
                Displacement = 156.9m,
                FuelCapacity = 7.8m,
                Description = "Honda SH 160i 2024 là biểu tượng của sự sang trọng và công nghệ tiên tiến.",
                ProductVariants = new List<ProductVariant>
                {
                    new ProductVariant { 
                        UrlSlug = "honda-sh-160i-2024-standard", 
                        Price = 92000000m, 
                        CoverImageUrl = "/assets/image/index/products/honda_sh160i_silver_studio_1775283413251.webp",
                        VersionName = "Tiêu chuẩn",
                        ColorName = "Bạc",
                        ColorCode = "#C0C0C0",
                        SKU = "SH160-2024-STD-SILVER"
                    },
                    new ProductVariant { 
                        UrlSlug = "honda-sh-160i-2024-premium", 
                        Price = 105000000m, 
                        CoverImageUrl = "/assets/image/index/products/honda_sh160i_silver_studio_1775283413251.webp",
                        VersionName = "Cao cấp",
                        ColorName = "Đen nhám",
                        ColorCode = "#1A1A1A",
                        SKU = "SH160-2024-PRE-BLACK"
                    }
                }
            },
            new Product
            {
                Name = "Yamaha Exciter 155 VVA",
                ShortDescription = "Kẻ thống trị đường phố, công nghệ VVA bứt phá.",
                CategoryId = bikeCategory.Id,
                BrandId = yamaha?.Id,
                StatusId = Domain.Constants.Product.ProductStatus.ForSale,
                EngineType = "4 kỳ, 4 van, SOHC, làm mát bằng dung dịch",
                Displacement = 155m,
                FuelCapacity = 5.4m,
                Description = "Yamaha Exciter 155 VVA mang DNA của dòng xe đua R1, cho khả năng tăng tốc ấn tượng.",
                ProductVariants = new List<ProductVariant>
                {
                    new ProductVariant { 
                        UrlSlug = "yamaha-exciter-155-vva-gp", 
                        Price = 52000000m, 
                        CoverImageUrl = "/assets/image/index/products/yamaha_nvx155_blue_studio_1775283455711.webp",
                        VersionName = "GP",
                        ColorName = "Xanh GP",
                        ColorCode = "#0000FF",
                        SKU = "EXC155-2024-GP-BLUE"
                    }
                }
            },
            new Product
            {
                Name = "Honda Winner X 2024",
                ShortDescription = "Bản lĩnh tay côn, thiết kế thể thao mạnh mẽ.",
                CategoryId = bikeCategory.Id,
                BrandId = honda?.Id,
                StatusId = Domain.Constants.Product.ProductStatus.ForSale,
                EngineType = "PGM-FI, 4 kỳ, DOHC, đơn xy-lanh, làm mát bằng dung dịch",
                Displacement = 149.1m,
                FuelCapacity = 4.5m,
                Description = "Honda Winner X 2024 với phanh ABS an toàn và kiểu dáng siêu xe thể thao.",
                ProductVariants = new List<ProductVariant>
                {
                    new ProductVariant { 
                        UrlSlug = "honda-winner-x-abs", 
                        Price = 46000000m, 
                        CoverImageUrl = "/assets/image/index/products/honda_winnerx_red_studio_1775283433602.webp",
                        VersionName = "Thể thao (ABS)",
                        ColorName = "Đỏ đen",
                        ColorCode = "#FF0000",
                        SKU = "WINX-2024-ABS-RED"
                    }
                }
            },
            new Product
            {
                Name = "Honda Vision 2024",
                ShortDescription = "Xe ga quốc dân, thanh lịch và tiết kiệm nhiên liệu.",
                CategoryId = bikeCategory.Id,
                BrandId = honda?.Id,
                StatusId = Domain.Constants.Product.ProductStatus.ForSale,
                Weight = 98m,
                Dimensions = "1.925 x 686 x 1.126 mm",
                Wheelbase = "1.255 mm",
                SeatHeight = 785m,
                GroundClearance = 130m,
                FuelCapacity = 4.9m,
                TireSize = "Trước: 80/90-16, Sau: 90/90-14",
                FrontSuspension = "Ống lồng, giảm chấn thủy lực",
                RearSuspension = "Lò xo trụ đơn, giảm chấn thủy lực",
                EngineType = "PGM-FI, Xăng, 4 kỳ, 1 xi-lanh, làm mát bằng không khí",
                MaxPower = "6,59 kW / 7.500 vòng/phút",
                OilCapacity = 0.65m,
                FuelConsumption = "1,85 L/100km",
                TransmissionType = "Tự động, biến thiên vô cấp",
                StarterSystem = "Điện",
                MaxTorque = "9,29 Nm / 6.000 vòng/phút",
                Displacement = 109.5m,
                BoreStroke = "47,0 x 63,1 mm",
                CompressionRatio = "10,0:1",
                Description = "Honda Vision là lựa chọn hàng đầu với thiết kế trẻ trung, động cơ eSP thông minh thế hệ mới, tiết kiệm nhiên liệu vượt trội.",
                ProductVariants = new List<ProductVariant>
                {
                    new ProductVariant { 
                        UrlSlug = "honda-vision-2024-standard", 
                        Price = 33000000m, 
                        CoverImageUrl = "/assets/image/index/products/honda_vision_white_studio_1775283476521.webp",
                        VersionName = "Tiêu chuẩn",
                        ColorName = "Trắng",
                        ColorCode = "#FFFFFF",
                        SKU = "VIS-2024-STD-WHITE"
                    }
                }
            }
        };

        var colors = await context.Set<OptionValue>().Include(v => v.Option).ToListAsync(cancellationToken).ConfigureAwait(false);
        var colorOption = await context.Set<Option>().FirstOrDefaultAsync(o => o.Name == "Color", cancellationToken).ConfigureAwait(false);
        var typeOption = await context.Set<Option>().FirstOrDefaultAsync(o => o.Name == "VehicleType", cancellationToken).ConfigureAwait(false);

        foreach (var p in productsToSeed)
        {
            var existing = await context.Products
                .Include(x => x.ProductVariants)
                .ThenInclude(v => v.VariantOptionValues)
                .FirstOrDefaultAsync(x => x.Name == p.Name, cancellationToken)
                .ConfigureAwait(false);

            if (existing == null)
            {
                await context.Products.AddAsync(p, cancellationToken).ConfigureAwait(false);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                existing = p;
            }

            // Assign options to variants
            foreach (var variant in existing.ProductVariants)
            {
                var pVariant = p.ProductVariants.FirstOrDefault(v => v.UrlSlug == variant.UrlSlug);
                if (pVariant != null)
                {
                    variant.VersionName = pVariant.VersionName;
                    variant.ColorName = pVariant.ColorName;
                    variant.ColorCode = pVariant.ColorCode;
                    variant.SKU = pVariant.SKU;
                    variant.Price = pVariant.Price;
                }

                if (!variant.VariantOptionValues.Any())
                {
                    // Assign a type
                    var typeName = p.Name.Contains("Vision") || p.Name.Contains("SH") ? "Xe ga" : "Tay côn";
                    if (typeOption != null)
                    {
                        var typeVal = await context.Set<OptionValue>().FirstOrDefaultAsync(v => v.OptionId == typeOption.Id && v.Name == typeName, cancellationToken);
                        if (typeVal != null)
                        {
                            variant.VariantOptionValues.Add(new VariantOptionValue { OptionValueId = typeVal.Id });
                        }
                    }

                    // Assign a color
                    var colorName = variant.UrlSlug.Contains("red") ? "Đỏ" : (variant.UrlSlug.Contains("blue") ? "Xanh" : "Trắng");
                    if (colorOption != null)
                    {
                        var colorVal = await context.Set<OptionValue>().FirstOrDefaultAsync(v => v.OptionId == colorOption.Id && v.Name == colorName, cancellationToken);
                        if (colorVal != null)
                        {
                            variant.VariantOptionValues.Add(new VariantOptionValue { OptionValueId = colorVal.Id });
                        }
                    }
                }
            }
            
            // Update technical specs
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
