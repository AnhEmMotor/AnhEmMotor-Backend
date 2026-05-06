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
        var protectedCategories = configuration.GetSection("ProtectedCategories").Get<List<CategorySeedModel>>() ?? [];
        if (protectedCategories.Count == 0)
            return;
        var existingCategories = await context.ProductCategories.ToListAsync(cancellationToken);
        var existingCategoryDict = existingCategories.ToDictionary(c => c.Name!, StringComparer.OrdinalIgnoreCase);
        var categoriesToAdd = new List<ProductCategory>();
        bool hasChanges = false;
        foreach (var seed in protectedCategories)
        {
            if (string.IsNullOrWhiteSpace(seed.Name))
                continue;
            if (existingCategoryDict.TryGetValue(seed.Name, out var existing))
            {
                if (string.IsNullOrEmpty(existing.CategoryGroup) || existing.CategoryGroup != seed.Group)
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
            }
        }
        if (categoriesToAdd.Count != 0)
        {
            await context.ProductCategories.AddRangeAsync(categoriesToAdd, cancellationToken);
            hasChanges = true;
        }
        if (hasChanges)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private class CategorySeedModel
    {
        public string? Name { get; set; }

        public string? Group { get; set; }
    }
}
