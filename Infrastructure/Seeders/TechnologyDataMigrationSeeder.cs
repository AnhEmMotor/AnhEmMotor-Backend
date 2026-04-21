using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infrastructure.Seeders
{
    public static class TechnologyDataMigrationSeeder
    {
        public static async Task MigrateExistingHighlightsAsync(ApplicationDBContext context)
        {
            // Only migrate if there are no ProductTechnologies yet
            if (await context.Set<ProductTechnology>().AnyAsync())
            {
                return;
            }

            var productsWithHighlights = await context.Products
                .Where(p => !string.IsNullOrEmpty(p.Highlights))
                .ToListAsync();

            if (!productsWithHighlights.Any()) return;

            var allTechs = await context.Set<Technology>().ToListAsync();
            var categories = await context.Set<TechnologyCategory>().ToListAsync();

            foreach (var product in productsWithHighlights)
            {
                try
                {
                    var highlights = JsonSerializer.Deserialize<List<HighlightJson>>(product.Highlights!);
                    if (highlights == null) continue;

                    int order = 1;
                    foreach (var hl in highlights)
                    {
                        // Try to find a matching technology by title or name
                        var tech = allTechs.FirstOrDefault(t => 
                            t.Name!.Equals(hl.Title, StringComparison.OrdinalIgnoreCase) || 
                            t.DefaultTitle!.Equals(hl.Title, StringComparison.OrdinalIgnoreCase));

                        if (tech == null)
                        {
                            // If not found, create a new one in the category
                            var categoryName = hl.Tag ?? "TECHNOLOGY";
                            var category = categories.FirstOrDefault(c => c.Name!.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                            
                            if (category == null)
                            {
                                category = new TechnologyCategory { Name = categoryName };
                                context.Set<TechnologyCategory>().Add(category);
                                await context.SaveChangesAsync();
                                categories.Add(category);
                            }

                            tech = new Technology
                            {
                                Name = hl.Title,
                                DefaultTitle = hl.Title,
                                DefaultDescription = hl.Description,
                                DefaultImageUrl = hl.Image,
                                CategoryId = category.Id
                            };
                            context.Set<Technology>().Add(tech);
                            await context.SaveChangesAsync();
                            allTechs.Add(tech);
                        }

                        // Add to product
                        context.Set<ProductTechnology>().Add(new ProductTechnology
                        {
                            ProductId = product.Id,
                            TechnologyId = tech.Id,
                            CustomTitle = tech.DefaultTitle == hl.Title ? null : hl.Title,
                            CustomDescription = tech.DefaultDescription == hl.Description ? null : hl.Description,
                            CustomImageUrl = tech.DefaultImageUrl == hl.Image ? null : hl.Image,
                            DisplayOrder = order++
                        });
                    }
                }
                catch
                {
                    // Skip invalid JSON
                }
            }

            await context.SaveChangesAsync();
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
