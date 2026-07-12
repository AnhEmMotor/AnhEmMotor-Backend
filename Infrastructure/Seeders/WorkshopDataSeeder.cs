using Infrastructure.DBContexts;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Seeders;

public static class WorkshopDataSeeder
{
    public static async Task SeedAsync(
        ApplicationDBContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
