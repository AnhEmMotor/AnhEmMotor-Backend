using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Seeders
{
    public static class TechnologySeeder
    {
        public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
        {
            if (!await context.TechnologyCategories.AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                var safety = new TechnologyCategory { Name = "An Toàn" };
                var engine = new TechnologyCategory { Name = "Động Cơ" };
                var utility = new TechnologyCategory { Name = "Tiện Ích" };
                context.TechnologyCategories.AddRange(safety, engine, utility);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                var abs = new Technology
                {
                    CategoryId = safety.Id,
                    Name = "ABS",
                    DefaultTitle = "Hệ thống chống bó cứng phanh ABS",
                    DefaultDescription =
                        "Giúp ổn định tư thế xe bằng cách chống khóa bánh, mang lại sự an tâm cho người lái trên mọi điều kiện địa hình.",
                    DefaultImageUrl = "http://localhost:5000/api/v1/MediaFile/view-image/products/placeholder-abs.webp"
                };
                var esp = new Technology
                {
                    CategoryId = engine.Id,
                    Name = "ESP+",
                    DefaultTitle = "Động cơ eSP+ 4 van",
                    DefaultDescription =
                        "Động cơ thế hệ mới eSP+ 4 van giúp tăng công suất hoạt động, vận hành êm ái, tiết kiệm nhiên liệu và thân thiện môi trường.",
                    DefaultImageUrl = "http://localhost:5000/api/v1/MediaFile/view-image/products/placeholder-esp.webp"
                };
                var smartkey = new Technology
                {
                    CategoryId = utility.Id,
                    Name = "Smartkey",
                    DefaultTitle = "Khóa thông minh Honda Smart Key",
                    DefaultDescription =
                        "Khóa thông minh kết hợp tính năng định vị xe và báo động chống trộm, mang lại sự tiện lợi và an tâm tuyệt đối.",
                    DefaultImageUrl =
                        "http://localhost:5000/api/v1/MediaFile/view-image/products/placeholder-smartkey.webp"
                };
                context.Technologies.AddRange(abs, esp, smartkey);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
