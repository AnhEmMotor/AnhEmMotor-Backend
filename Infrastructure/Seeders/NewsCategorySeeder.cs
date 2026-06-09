using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class NewsCategorySeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var predefinedCategories = new List<NewsCategory>
        {
            new() { Id = 1, Name = "Tư vấn mua xe", Slug = "tu-van-mua-xe", IsActive = true },
            new() { Id = 2, Name = "Kinh nghiệm bảo dưỡng", Slug = "kinh-nghiem-bao-duong", IsActive = true },
            new() { Id = 3, Name = "Tin tức showroom", Slug = "tin-tuc-showroom", IsActive = true },
            new() { Id = 4, Name = "So sánh xe", Slug = "so-sanh-xe", IsActive = true }
        };

        var existingIds = await context.NewsCategories.Select(c => c.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        
        var missingCategories = predefinedCategories.Where(c => !existingIds.Contains(c.Id)).ToList();

        if (missingCategories.Count > 0)
        {
            var isSqlServer = context.Database.ProviderName?.Contains("SqlServer") == true;

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (isSqlServer)
                {
                    await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[NewsCategory] ON", cancellationToken).ConfigureAwait(false);
                }

                context.NewsCategories.AddRange(missingCategories);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (isSqlServer)
                {
                    await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[NewsCategory] OFF", cancellationToken).ConfigureAwait(false);
                }

                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }
    }
}
