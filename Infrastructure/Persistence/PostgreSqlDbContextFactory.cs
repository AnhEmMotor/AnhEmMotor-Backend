using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence
{
    public class PostgreSqlDbContextFactory : IDesignTimeDbContextFactory<PostgreSqlDbContext>
    {
        public PostgreSqlDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var webApiPath = Path.Combine(basePath, "WebAPI");

            if(Directory.Exists(webApiPath))
            {
                basePath = webApiPath;
            } else if(Directory.Exists(Path.Combine(basePath, "../WebAPI")))
            {
                basePath = Path.Combine(basePath, "../WebAPI");
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("StringConnection");

            if(string.IsNullOrEmpty(connectionString) ||
                connectionString.Contains("Server") is false ||
                connectionString.Contains("Port") is false)
            {
                connectionString = "Host=localhost;Port=5432;Database=anhemmotor;Username=postgres;Password=postgres;";
            }

            var builder = new DbContextOptionsBuilder<PostgreSqlDbContext>();
            builder.UseNpgsql(connectionString, b => b.MigrationsAssembly("Infrastructure"));

            return new PostgreSqlDbContext(builder.Options);
        }
    }
}
