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
        var protectedCategories = configuration.GetSection("ProtectedProductCategory").Get<List<string>>() ?? [];

        if(protectedCategories.Count == 0)
        {
            return;
        }

        // Fetch all existing category names into memory first to avoid complex LINQ translation issues with "Contains"
        var allExistingCategoryNames = await context.ProductCategories
            .Where(c => c.Name != null)
            .Select(c => c.Name!)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var existingCategorySet = new HashSet<string>(allExistingCategoryNames, StringComparer.OrdinalIgnoreCase);

        var categoriesToAdd = protectedCategories
            .Where(name => !string.IsNullOrWhiteSpace(name) && !existingCategorySet.Contains(name))
            .Select(name => new ProductCategory { Name = name, })
            .ToList();

        if(categoriesToAdd.Count != 0)
        {
            await context.ProductCategories.AddRangeAsync(categoriesToAdd, cancellationToken).ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
