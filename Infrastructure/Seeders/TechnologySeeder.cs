using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class TechnologySeeder
{
    public static async Task SeedAsync(ApplicationDBContext context)
    {
        // 1. Ensure Categories exist
        var categoryNames = new[] { "An toàn", "Tiện ích & Kết nối", "Động cơ & Vận hành" };
        foreach (var name in categoryNames)
        {
            if (!await context.TechnologyCategories.AnyAsync(c => c.Name == name))
            {
                context.TechnologyCategories.Add(new TechnologyCategory { Name = name });
            }
        }
        await context.SaveChangesAsync();

        var categories = await context.TechnologyCategories
            .ToDictionaryAsync(c => c.Name!.Trim(), c => c.Id, StringComparer.OrdinalIgnoreCase);

        // 2. Fetch Brands
        var honda = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Honda");
        var yamaha = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Yamaha");
        var piaggio = await context.Brands.FirstOrDefaultAsync(b => b.Name == "Piaggio");

        if (piaggio == null)
        {
            piaggio = new Brand { Name = "Piaggio" };
            context.Brands.Add(piaggio);
            await context.SaveChangesAsync();
        }

        // 3. Define Brand-Specific Technologies
        var techData = new List<(string Name, string Description, string Category, Brand? Brand)>
        {
            // HONDA
            ("eSP+ (4 Van)", "Động cơ thế hệ mới, chạy cực êm và tiết kiệm xăng.", "Động cơ & Vận hành", honda),
            ("HSTC", "Kiểm soát lực kéo, chống xoè khi đi đường trơn (SH, ADV).", "An toàn", honda),
            ("E-Clutch", "Côn tay điện tử (Công nghệ mới nhất, không cần bóp côn).", "Động cơ & Vận hành", honda),
            
            // YAMAHA
            ("VVA (Variable Valve)", "Van biến thiên, giúp xe \"bốc\" ở tốc độ cao (Exciter, NVX).", "Động cơ & Vận hành", yamaha),
            ("Blue Core", "Triết lý động cơ tập trung vào sự bền bỉ và tiết kiệm.", "Động cơ & Vận hành", yamaha),
            ("Y-Connect", "Kết nối điện thoại chuyên sâu nhất (báo lỗi, lịch bảo trì).", "Tiện ích & Kết nối", yamaha),

            // PIAGGIO
            ("i-Get", "Động cơ thế hệ mới giúp giảm tiếng ồn và độ rung (Vespa, Liberty).", "Động cơ & Vận hành", piaggio),
            ("MIA", "Hệ thống định vị và kết nối smartphone cao cấp.", "Tiện ích & Kết nối", piaggio),

            // GENERIC (Optional based on previous seeder but kept for completeness)
            ("ABS 1 Kênh", "Hệ thống chống bó cứng phanh cho bánh trước.", "An toàn", null),
            ("ABS 2 Kênh", "Hệ thống chống bó cứng phanh cho cả 2 bánh.", "An toàn", null),
            ("Smartkey", "Hệ thống khóa thông minh chống trộm.", "Tiện ích & Kết nối", null),
            ("Hệ thống đèn LED", "Chiếu sáng mạnh mẽ và tiết kiệm điện năng.", "Tiện ích & Kết nối", null)
        };

        foreach (var data in techData)
        {
            var exists = await context.Technologies
                .AnyAsync(t => t.Name == data.Name && t.BrandId == (data.Brand != null ? data.Brand.Id : null));

            if (!exists)
            {
                if (categories.TryGetValue(data.Category.Trim(), out var categoryId))
                {
                    context.Technologies.Add(new Technology
                    {
                        Name = data.Name,
                        DefaultTitle = data.Name,
                        DefaultDescription = data.Description,
                        CategoryId = categoryId,
                        BrandId = data.Brand?.Id
                    });
                }
            }
            else 
            {
                // Update existing one to ensure description matches the image
                var tech = await context.Technologies
                    .FirstOrDefaultAsync(t => t.Name == data.Name && t.BrandId == (data.Brand != null ? data.Brand.Id : null));
                if (tech != null && categories.TryGetValue(data.Category.Trim(), out var categoryId))
                {
                    tech.DefaultDescription = data.Description;
                    tech.CategoryId = categoryId;
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
