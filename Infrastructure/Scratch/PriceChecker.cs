
namespace Infrastructure.Scratch;

using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class PriceChecker
{
    public static async Task CheckPrices(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var variants = await context.ProductVariants
            .Select(v => new { v.Id, v.Price, Name = v.Product!.Name })
            .Take(10)
            .ToListAsync();
        foreach (var v in variants)
        {
            Console.WriteLine($"Product: {v.Name}, Price: {v.Price}");
        }
    }
}
