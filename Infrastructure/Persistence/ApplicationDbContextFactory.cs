using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDBContext>
{
    public ApplicationDBContext CreateDbContext(string[] args)
    {
        throw new InvalidOperationException(
            "Cannot create migration using ApplicationDBContext. Please use a specific provider DbContext (SqlServerDBContext, MySqlDbContext, or PostgreSqlDbContext).");
    }
}
