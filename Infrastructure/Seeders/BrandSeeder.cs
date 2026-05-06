using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class BrandSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var brands = new List<Brand>
        {
            new() { Name = "Honda" },
            new() { Name = "Yamaha" },
            new() { Name = "Suzuki" },
            new() { Name = "Piaggio" },
            new() { Name = "Kawasaki" }
        };
        foreach (var brand in brands)
        {
            var existing = await context.Brands
                .FirstOrDefaultAsync(b => string.Compare(b.Name, brand.Name) == 0, cancellationToken)
                .ConfigureAwait(false);
            if (existing == null)
            {
                await context.Brands.AddAsync(brand, cancellationToken).ConfigureAwait(false);
            }
        }
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
