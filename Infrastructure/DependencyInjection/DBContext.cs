using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Repositories.File;
using Application.Interfaces.Repositories.Setting;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services.Brand;
using Application.Interfaces.Services.File;
using Application.Interfaces.Services.Setting;
using Application.Interfaces.Services.Supplier;
using Application.Services.Brand;
using Application.Services.File;
using Application.Services.Setting;
using Application.Services.Supplier;
using Application.Sieve;
using Infrastructure.DBContexts;
using Infrastructure.Repositories.Brand;
using Infrastructure.Repositories.File;
using Infrastructure.Repositories.Setting;
using Infrastructure.Repositories.Supplier;
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

            services.AddScoped<ISupplierInsertRepository, SupplierInsertRepository>();
            services.AddScoped<ISupplierSelectRepository, SupplierSelectRepository>();
            services.AddScoped<ISupplierUpdateRepository, SupplierUpdateRepository>();
            services.AddScoped<ISupplierDeleteRepository, SupplierDeleteRepository>();

            services.AddScoped<ISupplierInsertService, SupplierInsertService>();
            services.AddScoped<ISupplierSelectService, SupplierSelectService>();
            services.AddScoped<ISupplierUpdateService, SupplierUpdateService>();
            services.AddScoped<ISupplierDeleteService, SupplierDeleteService>();

            services.AddScoped<ISettingRepository, SettingRepository>();
            services.AddScoped<ISettingService, SettingService>();

            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<IMediaFileRepository, MediaFileRepository>();
            services.AddScoped<IFileService, FileService>();

            services.AddScoped<ISieveProcessor, CustomSieveProcessor>();

            return services;
        }
    }
}
