using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infrastructure.Seeders
{
    public static class TechnologyDataMigrationSeeder
    {
        public static async Task MigrateExistingHighlightsAsync(
            ApplicationDBContext context,
            CancellationToken cancellationToken)
        {
            if (await context.Set<ProductTechnology>().AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                return;
            }
            var productsWithHighlights = await context.Products
                .Where(p => !string.IsNullOrEmpty(p.Highlights))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            if (productsWithHighlights.Count == 0)
                return;
            var allTechs = await context.Set<Technology>().ToListAsync(cancellationToken).ConfigureAwait(false);
            var categories = await context.Set<TechnologyCategory>()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (var product in productsWithHighlights)
            {
                try
                {
                    var highlights = JsonSerializer.Deserialize<List<HighlightJson>>(product.Highlights!);
                    if (highlights == null)
                        continue;
                    int order = 1;
                    foreach (var hl in highlights)
                    {
                        var tech = allTechs.FirstOrDefault(
                            t => t.Name!.Equals(hl.Title, StringComparison.OrdinalIgnoreCase) ||
                                t.DefaultTitle!.Equals(hl.Title, StringComparison.OrdinalIgnoreCase));
                        if (tech == null)
                        {
                            var categoryName = hl.Tag ?? "TECHNOLOGY";
                            var category = categories.FirstOrDefault(
                                c => c.Name!.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                            if (category == null)
                            {
                                category = new TechnologyCategory { Name = categoryName };
                                context.Set<TechnologyCategory>().Add(category);
                                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                                categories.Add(category);
                            }
                            tech = new Technology
                            {
                                Name = hl.Title ?? string.Empty,
                                DefaultTitle = hl.Title,
                                DefaultDescription = hl.Description,
                                DefaultImageUrl = hl.Image,
                                CategoryId = category.Id
                            };
                            context.Set<Technology>().Add(tech);
                            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            allTechs.Add(tech);
                        }
                        context.Set<ProductTechnology>()
                            .Add(
                                new ProductTechnology
                                {
                                    ProductId = product.Id,
                                    TechnologyId = tech.Id,
                                    CustomTitle = string.Compare(tech.DefaultTitle, hl.Title) == 0 ? null : hl.Title,
                                    CustomDescription =
                                        string.Compare(tech.DefaultDescription, hl.Description) == 0
                                                ? null
                                                : hl.Description,
                                    CustomImageUrl =
                                        string.Compare(tech.DefaultImageUrl, hl.Image) == 0 ? null : hl.Image,
                                    DisplayOrder = order++
                                });
                    }
                } catch
                {
                }
            }
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private class HighlightJson
        {
            public string? Title { get; set; }

            public string? Description { get; set; }

            public string? Image { get; set; }

            public string? Tag { get; set; }
        }
    }
}
