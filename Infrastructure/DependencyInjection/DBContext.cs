using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Services.Brand;
using Application.Services.Brand;
using Application.Sieve;
using Infrastructure.DBContexts;
using Infrastructure.Repositories.Brand;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Services;

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

            services.AddScoped<IBrandInsertRepository, BrandInsertRepository>();
            services.AddScoped<IBrandSelectRepository, BrandSelectRepository>();
            services.AddScoped<IBrandUpdateRepository, BrandUpdateRepository>();
            services.AddScoped<IBrandDeleteRepository, BrandDeleteRepository>();

            services.AddScoped<IBrandInsertService, BrandInsertService>();
            services.AddScoped<IBrandSelectService, BrandSelectService>();
            services.AddScoped<IBrandUpdateService, BrandUpdateService>();
            services.AddScoped<IBrandDeleteService, BrandDeleteService>();

            services.AddScoped<ISieveProcessor, BrandSieveProcessor>();

            return services;
        }
    }
}