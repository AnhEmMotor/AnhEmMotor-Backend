using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Seeders;

public static class ProductCategorySeeder
{
    public static async Task SeedAsync(
        ApplicationDBContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var seedData = configuration.GetSection("ProtectedCategories").Get<List<CategorySeedModel>>() ?? [];
        if (seedData.Count == 0)
            return;
        var existingCategories = await context.ProductCategories.ToListAsync(cancellationToken).ConfigureAwait(false);
        var existingCategoryDict = existingCategories
            .Where(c => c.Name != null)
            .ToDictionary(c => c.Name!, StringComparer.OrdinalIgnoreCase);
        var categoriesToAdd = new List<ProductCategory>();
        bool hasChanges = false;
        foreach (var seed in seedData)
        {
            if (string.IsNullOrWhiteSpace(seed.Name))
                continue;
            if (existingCategoryDict.TryGetValue(seed.Name, out var existing))
            {
                if (!string.Equals(existing.CategoryGroup, seed.Group, StringComparison.OrdinalIgnoreCase))
                {
                    existing.CategoryGroup = seed.Group;
                    hasChanges = true;
                }
            } else
            {
                categoriesToAdd.Add(
                    new ProductCategory
                    {
                        Name = seed.Name,
                        CategoryGroup = seed.Group,
                        Slug = seed.Name.ToLower().Replace(" ", "-")
                    });
                hasChanges = true;
            }
        }
        if (categoriesToAdd.Count > 0)
        {
            await context.ProductCategories.AddRangeAsync(categoriesToAdd, cancellationToken).ConfigureAwait(false);
        }
        if (hasChanges)
        {
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private class CategorySeedModel
    {
        public string? Name { get; set; }

        public string? Group { get; set; }
    }
}