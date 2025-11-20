using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Repositories.File;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Repositories.Setting;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services.Brand;
using Application.Interfaces.Services.File;
using Application.Interfaces.Services.Product;
using Application.Interfaces.Services.ProductCategory;
using Application.Interfaces.Services.Setting;
using Application.Interfaces.Services.Supplier;
using Application.Services.Brand;
using Application.Services.File;
using Application.Services.Product;
using Application.Services.ProductCategory;
using Application.Services.Setting;
using Application.Services.Supplier;
using Application.Sieve;
using Infrastructure.DBContexts;
using Infrastructure.Repositories.Brand;
using Infrastructure.Repositories.File;
using Infrastructure.Repositories.Product;
using Infrastructure.Repositories.ProductCategory;
using Infrastructure.Repositories.Setting;
using Infrastructure.Repositories.Supplier;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Services;

namespace Infrastructure.DependencyInjection;

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

        services.AddScoped<IUnitOfWork, UnitOfWork>();

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

        services.AddScoped<IProductCategorySelectRepository, ProductCategorySelectRepository>();
        services.AddScoped<IProductCategoryInsertRepository, ProductCategoryInsertRepository>();
        services.AddScoped<IProductCategoryUpdateRepository, ProductCategoryUpdateRepository>();
        services.AddScoped<IProductCategoryDeleteRepository, ProductCategoryDeleteRepository>();

        services.AddScoped<IProductCategorySelectService, ProductCategorySelectService>();
        services.AddScoped<IProductCategoryInsertService, ProductCategoryInsertService>();
        services.AddScoped<IProductCategoryUpdateService, ProductCategoryUpdateService>();
        services.AddScoped<IProductCategoryDeleteService, ProductCategoryDeleteService>();

        services.AddScoped<IProductSelectRepository, ProductSelectRepository>();
        services.AddScoped<IProductInsertRepository, ProductInsertRepository>();
        services.AddScoped<IProductUpdateRepository, ProductUpdateRepository>();
        services.AddScoped<IProductDeleteRepository, ProductDeleteRepository>();

        services.AddScoped<IProductSelectService, ProductSelectService>();
        services.AddScoped<IProductInsertService, ProductInsertService>();
        services.AddScoped<IProductUpdateService, ProductUpdateService>();
        services.AddScoped<IProductDeleteService, ProductDeleteService>();

        services.AddScoped<ISettingRepository, SettingRepository>();
        services.AddScoped<ISettingService, SettingService>();

        services.AddScoped<IFileRepository, FileRepository>();

        services.AddScoped<IMediaFileInsertRepository, MediaFileInsertRepository>();
        services.AddScoped<IMediaFileSelectRepository, MediaFileSelectRepository>();
        services.AddScoped<IMediaFileDeleteRepository, MediaFileDeleteRepository>();
        services.AddScoped<IMediaFileRestoreRepository, MediaFileRestoreRepository>();

        services.AddScoped<IFileInsertService, FileInsertService>();
        services.AddScoped<IFileSelectService, FileSelectService>();
        services.AddScoped<IFileDeleteService, FileDeleteService>();
        services.AddScoped<IFileUpdateService, FileUpdateService>();

        services.AddScoped<ISieveProcessor, CustomSieveProcessor>();

        return services;
    }
}
