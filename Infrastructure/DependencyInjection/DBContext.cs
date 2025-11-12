using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection
{
    public static class DBContext
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDBContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("StringConnection"), b =>
                {
                    b.MigrationsAssembly("Infrastructure");
                    b.CommandTimeout(2);
                });
            });

            // Đăng ký các Repositories (khi bạn tạo chúng)
            // Ví dụ:
            // services.AddScoped<IProductRepository, ProductRepository>();
            // services.AddScoped<IBrandRepository, BrandRepository>();

            return services;
        }
    }
}