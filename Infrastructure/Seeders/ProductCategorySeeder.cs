using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Seeders;

/// <summary>
/// Seeder để khởi tạo các danh mục sản phẩm được bảo vệ từ appsettings
/// </summary>
public static class ProductCategorySeeder
{
    /// <summary>
    /// Seed các danh mục sản phẩm được bảo vệ từ appsettings
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="configuration">Configuration để đọc ProtectedProductCategory</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task SeedAsync(
        ApplicationDBContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var protectedCategories = configuration.GetSection("ProtectedProductCategory").Get<List<string>>() ?? [];

        if (protectedCategories.Count == 0)
        {
            return;
        }

        var existingCategories = await context.ProductCategories
            .Where(pc => pc.Name != null && protectedCategories.Contains(pc.Name))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var existingCategoryNames = existingCategories
            .Where(c => c.Name is not null)
            .Select(c => c.Name!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var categoriesToAdd = protectedCategories
            .Where(name => !string.IsNullOrWhiteSpace(name) && 
                          !existingCategoryNames.Contains(name))
            .Select(name => new ProductCategory
            {
                Name = name,
            })
            .ToList();

        if (categoriesToAdd.Count != 0)
        {
            await context.ProductCategories
                .AddRangeAsync(categoriesToAdd, cancellationToken)
                .ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
