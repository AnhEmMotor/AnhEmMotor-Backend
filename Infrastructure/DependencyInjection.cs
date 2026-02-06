using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.LocalFile;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Authorization;
using Infrastructure.Authorization.Hander;
using Infrastructure.DBContexts;
using Infrastructure.Repositories;
using Infrastructure.Repositories.LocalFile;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.EntityFrameworkCore.Extensions;
using System.Reflection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration.GetValue("Provider", "SqlServer");

        if (provider == "MySql")
        {
            var connectionString = configuration.GetConnectionString("StringConnection") ?? "";
            services.AddDbContextPool<ApplicationDBContext, MySqlDbContext>(options =>
            {
                options.UseMySQL(
                    connectionString);
            });
        }
        else
        {
            services.AddDbContextPool<ApplicationDBContext>(
                options =>
                {
                    options.UseSqlServer(
                        configuration.GetConnectionString("StringConnection"),
                        b => b.MigrationsAssembly(typeof(ApplicationDBContext).Assembly.FullName).CommandTimeout(30));
                });
        }

        services.AddIdentity<ApplicationUser, ApplicationRole>(
            options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDBContext>()
            .AddDefaultTokenProviders();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddScoped<IAuthorizationHandler, AllPermissionsHandler>();
        services.AddScoped<IAuthorizationHandler, AnyPermissionsHandler>();

        services.AddScoped<ITokenManagerService, TokenManagerService>();
        services.AddScoped<IHttpTokenAccessorService, HttpTokenAccessorService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IProtectedEntityManagerService, ProtectedEntityManagerService>();
        services.AddScoped<IProtectedProductCategoryService, ProtectedProductCategoryService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<ISievePaginator, SievePaginator>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Scan(
            scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}