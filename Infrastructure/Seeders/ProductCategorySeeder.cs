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

        var existingCategories = await context.ProductCategories
            .Where(pc => pc.Name != null && protectedCategories.Contains(pc.Name))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var existingCategoryNames = existingCategories
            .Where(c => c.Name is not null)
            .Select(c => c.Name!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var categoriesToAdd = protectedCategories
            .Where(name => !string.IsNullOrWhiteSpace(name) && !existingCategoryNames.Contains(name))
            .Select(name => new ProductCategory { Name = name, })
            .ToList();

        if(categoriesToAdd.Count != 0)
        {
            await context.ProductCategories.AddRangeAsync(categoriesToAdd, cancellationToken).ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
