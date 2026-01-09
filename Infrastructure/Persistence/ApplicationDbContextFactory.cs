using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDBContext>
    {
        public ApplicationDBContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../WebAPI");

            if (!Directory.Exists(basePath))
            {
                basePath = Directory.GetCurrentDirectory();
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("StringConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Không tìm thấy ConnectionString 'StringConnection'.");
            }

            var builder = new DbContextOptionsBuilder<ApplicationDBContext>();
            builder.UseSqlServer(connectionString, b => b.MigrationsAssembly("Infrastructure"));

            return new ApplicationDBContext(builder.Options);
        }
    }
}