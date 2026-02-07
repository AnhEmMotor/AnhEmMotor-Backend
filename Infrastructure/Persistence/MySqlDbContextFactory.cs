using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence
{
    public class MySqlDbContextFactory : IDesignTimeDbContextFactory<MySqlDbContext>
    {
        public MySqlDbContext CreateDbContext(string[] args)
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
                connectionString.Contains("Initial Catalog") ||
                connectionString.Contains("Data Source"))
            {
                connectionString = "Server=localhost;Database=anhemmotor;User=root;Password=root;";
            }

            var builder = new DbContextOptionsBuilder<MySqlDbContext>();

            var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));

            builder.UseMySql(connectionString, serverVersion, b => b.MigrationsAssembly("Infrastructure"));

            return new MySqlDbContext(builder.Options);
        }
    }
}