using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence
{
    public class SqlServerDbContextFactory : IDesignTimeDbContextFactory<SqlServerDBContext>
    {
        public SqlServerDBContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var webApiPath = Path.Combine(basePath, "WebAPI");
            if (Directory.Exists(webApiPath))
            {
                basePath = webApiPath;
            } else if (Directory.Exists(Path.Combine(basePath, "../WebAPI")))
            {
                basePath = Path.Combine(basePath, "../WebAPI");
            }
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            var connectionString = configuration.GetConnectionString("StringConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Could not find ConnectionString 'StringConnection'.");
            }
            var builder = new DbContextOptionsBuilder<SqlServerDBContext>();
            builder.UseSqlServer(connectionString, b => b.MigrationsAssembly("Infrastructure"));
            return new SqlServerDBContext(builder.Options);
        }
    }
}
