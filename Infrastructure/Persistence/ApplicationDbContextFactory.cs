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
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=========================================================================================");
            Console.WriteLine("CẢNH BÁO: Không được tạo migration trực tiếp bằng ApplicationDBContext!");
            Console.WriteLine("Bạn bắt buộc phải sử dụng một trong các DbContext cụ thể sau:");
            Console.WriteLine(" - SqlServerDBContext");
            Console.WriteLine(" - MySqlDbContext");
            Console.WriteLine(" - PostgreSqlDbContext");
            Console.WriteLine();
            Console.WriteLine("Ví dụ command:");
            Console.WriteLine("dotnet ef migrations add <MigrationName> --context SqlServerDBContext --output-dir Migrations/SqlServerMigrations");
            Console.WriteLine("=========================================================================================");
            Console.ResetColor();

            throw new InvalidOperationException("Cannot create migration using ApplicationDBContext. Please use a specific provider DbContext (SqlServerDBContext, MySqlDbContext, or PostgreSqlDbContext).");
        }
    }
}
