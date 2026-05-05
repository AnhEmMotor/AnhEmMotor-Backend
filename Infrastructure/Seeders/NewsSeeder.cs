using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class NewsSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var newsList = new List<News>
        {
            new() {
                Title = "Khai trương showroom mới tại TP.HCM",
                Slug = "khai-truong-showroom-moi-tphcm",
                Content = "Chúng tôi vui mừng thông báo khai trương showroom mới với nhiều ưu đãi hấp dẫn...",
                AuthorName = "Admin",
                IsPublished = true,
                PublishedDate = DateTimeOffset.UtcNow,
                CoverImageUrl = "/assets/image/news/news-1.webp"
            },
            new() {
                Title = "Ưu đãi cực khủng tháng 4 cho dòng xe tay ga",
                Slug = "uu-dai-thang-4-xe-tay-ga",
                Content = "Giảm ngay 5 triệu đồng khi mua các dòng xe tay ga Honda, Yamaha trong tháng 4...",
                AuthorName = "Marketing",
                IsPublished = true,
                PublishedDate = DateTimeOffset.UtcNow,
                CoverImageUrl = "/assets/image/news/news-2.webp"
            },
            new() {
                Title = "Hướng dẫn bảo dưỡng xe máy định kỳ đúng cách",
                Slug = "huong-dan-bao-duong-xe-may",
                Content = "Việc bảo dưỡng xe máy định kỳ giúp kéo dài tuổi thọ và đảm bảo an toàn...",
                AuthorName = "Kỹ thuật",
                IsPublished = true,
                PublishedDate = DateTimeOffset.UtcNow,
                CoverImageUrl = "/assets/image/news/news-3.webp"
            }
        };
        foreach (var news in newsList)
        {
            var existing = await context.News
                .FirstOrDefaultAsync(n => string.Compare(n.Slug, news.Slug) == 0, cancellationToken)
                .ConfigureAwait(false);
            if (existing == null)
            {
                await context.News.AddAsync(news, cancellationToken).ConfigureAwait(false);
            }
        }
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
