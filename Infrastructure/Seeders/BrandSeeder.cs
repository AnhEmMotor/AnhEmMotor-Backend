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
            new Brand { Name = "Honda" },
            new Brand { Name = "Yamaha" },
            new Brand { Name = "Suzuki" },
            new Brand { Name = "Piaggio" },
            new Brand { Name = "Kawasaki" }
        };
        foreach (var brand in brands)
        {
            var existing = await context.Brands
                .FirstOrDefaultAsync(b => b.Name == brand.Name, cancellationToken)
                .ConfigureAwait(false);
            if (existing == null)
            {
                await context.Brands.AddAsync(brand, cancellationToken).ConfigureAwait(false);
            }
        }
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
