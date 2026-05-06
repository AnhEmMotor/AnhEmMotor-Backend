namespace Infrastructure.Scratch;

using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

public class OrderChecker
{
    public static async Task CheckOrders(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var orders = await context.OutputOrders
            .Select(o => new { o.Id, o.StatusId, o.CustomerName })
            .ToListAsync();
            
        Console.WriteLine("--- List of Orders in DB ---");
        foreach (var o in orders)
        {
            Console.WriteLine($"Order ID: {o.Id}, Status: {o.StatusId}, Buyer: {o.CustomerName}");
        }
        Console.WriteLine("----------------------------");
    }
}
