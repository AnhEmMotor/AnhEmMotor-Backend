using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Repositories.File;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Setting;
using Application.Interfaces.Repositories.Supplier;
using Application.Sieve;
using Infrastructure.DBContexts;
using Infrastructure.Repositories.Brand;
using Infrastructure.Repositories.File;
using Infrastructure.Repositories.Product;
using Infrastructure.Repositories.ProductCategory;
using Infrastructure.Repositories.ProductVariant;
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
        services.AddScoped<IBrandReadRepository, BrandReadRepository>();
        services.AddScoped<IBrandUpdateRepository, BrandUpdateRepository>();
        services.AddScoped<IBrandDeleteRepository, BrandDeleteRepository>();

        services.AddScoped<ISupplierInsertRepository, SupplierInsertRepository>();
        services.AddScoped<ISupplierReadRepository, SupplierReadRepository>();
        services.AddScoped<ISupplierUpdateRepository, SupplierUpdateRepository>();
        services.AddScoped<ISupplierDeleteRepository, SupplierDeleteRepository>();

        services.AddScoped<IProductCategoryReadRepository, ProductCategoryReadRepository>();
        services.AddScoped<IProductCategoryInsertRepository, ProductCategoryInsertRepository>();
        services.AddScoped<IProductCategoryUpdateRepository, ProductCategoryUpdateRepository>();
        services.AddScoped<IProductCategoryDeleteRepository, ProductCategoryDeleteRepository>();

        services.AddScoped<IProductSelectRepository, ProductSelectRepository>();
        services.AddScoped<IProductInsertRepository, ProductInsertRepository>();
        services.AddScoped<IProductUpdateRepository, ProductUpdateRepository>();
        services.AddScoped<IProductDeleteRepository, ProductDeleteRepository>();

        services.AddScoped<ISettingRepository, SettingRepository>();

        services.AddScoped<IFileRepository, FileRepository>();

        services.AddScoped<IMediaFileInsertRepository, MediaFileInsertRepository>();
        services.AddScoped<IMediaFileSelectRepository, MediaFileSelectRepository>();
        services.AddScoped<IMediaFileDeleteRepository, MediaFileDeleteRepository>();
        services.AddScoped<IMediaFileRestoreRepository, MediaFileRestoreRepository>();

        services.AddScoped<IProductVariantUpdateRepository, ProductVariantUpdateRepository>();

        return services;
    }
}
