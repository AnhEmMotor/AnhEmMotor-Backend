using Application.Interfaces.Authentication;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Repositories.Input;
using Application.Interfaces.Repositories.LocalFile;
using Application.Interfaces.Repositories.MediaFile;
using Application.Interfaces.Repositories.Option;
using Application.Interfaces.Repositories.OptionValue;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Setting;
using Application.Interfaces.Repositories.Statistical;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Repositories.VariantOptionValue;

using Infrastructure.Authorization;
using Infrastructure.DBContexts;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Brand;
using Infrastructure.Repositories.Input;
using Infrastructure.Repositories.LocalFile;
using Infrastructure.Repositories.MediaFile;
using Infrastructure.Repositories.Option;
using Infrastructure.Repositories.OptionValue;
using Infrastructure.Repositories.Output;
using Infrastructure.Repositories.Product;
using Infrastructure.Repositories.ProductCategory;
using Infrastructure.Repositories.ProductVariant;
using Infrastructure.Repositories.Setting;
using Infrastructure.Repositories.Statistical;
using Infrastructure.Repositories.Supplier;
using Infrastructure.Repositories.VariantOptionValue;
using Infrastructure.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class DBContext
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDBContext>(
            options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("StringConnection"),
                    b =>
                    {
                        b.MigrationsAssembly("Infrastructure");
                        b.CommandTimeout(2);
                    });
            });

        // Configure Identity
        
       

        // Configure Authorization with custom policy provider
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddScoped<IAuthorizationHandler, AllPermissionsHandler>();
        services.AddScoped<IAuthorizationHandler, AnyPermissionsHandler>();

        // Register TokenService
        services.AddScoped<TokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

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

        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<IProductInsertRepository, ProductInsertRepository>();
        services.AddScoped<IProductUpdateRepository, ProductUpdateRepository>();
        services.AddScoped<IProductDeleteRepository, ProductDeleteRepository>();

        services.AddScoped<IOptionReadRepository, OptionReadRepository>();

        services.AddScoped<IOptionValueReadRepository, OptionValueReadRepository>();
        services.AddScoped<IOptionValueInsertRepository, OptionValueInsertRepository>();
        services.AddScoped<IOptionValueDeleteRepository, OptionValueDeleteRepository>();

        services.AddScoped<IProductVariantInsertRepository, ProductVariantInsertRepository>();
        services.AddScoped<IProductVariantReadRepository, ProductVariantReadRepository>();
        services.AddScoped<IProductVariantUpdateRepository, ProductVariantUpdateRepository>();
        services.AddScoped<IProductVarientDeleteRepository, ProductVarientDeleteRepository>();

        services.AddScoped<IVariantOptionValueDeleteRepository, VariantOptionValueDeleteRepository>();

        services.AddScoped<ISettingRepository, SettingRepository>();

        services.AddScoped<IMediaFileReadRepository, MediaFileReadRepository>();
        services.AddScoped<IMediaFileInsertRepository, MediaFileInsertRepository>();
        services.AddScoped<IMediaFileUpdateRepository, MediaFileUpdateRepository>();
        services.AddScoped<IMediaFileDeleteRepository, MediaFileDeleteRepository>();

        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        services.AddScoped<IPaginator, SievePaginator>();

        services.AddScoped<IInputReadRepository, InputReadRepository>();
        services.AddScoped<IInputInsertRepository, InputInsertRepository>();
        services.AddScoped<IInputUpdateRepository, InputUpdateRepository>();
        services.AddScoped<IInputDeleteRepository, InputDeleteRepository>();

        services.AddScoped<IOutputReadRepository, OutputReadRepository>();
        services.AddScoped<IOutputInsertRepository, OutputInsertRepository>();
        services.AddScoped<IOutputUpdateRepository, OutputUpdateRepository>();
        services.AddScoped<IOutputDeleteRepository, OutputDeleteRepository>();

        services.AddScoped<IStatisticalReadRepository, StatisticalReadRepository>();

        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
