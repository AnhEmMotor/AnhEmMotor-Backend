using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.LocalFile;
using Application.Interfaces.Services;
using Infrastructure.Authorization;
using Infrastructure.Authorization.Hander;
using Infrastructure.DBContexts;
using Infrastructure.Repositories;
using Infrastructure.Repositories.LocalFile;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Infrastructure.DependencyInjection;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContextPool<ApplicationDBContext>(
            options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("StringConnection"),
                    b =>
                    {
                        b.MigrationsAssembly("Infrastructure");
                        b.CommandTimeout(30);
                    });
            });

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddScoped<IAuthorizationHandler, AllPermissionsHandler>();
        services.AddScoped<IAuthorizationHandler, AnyPermissionsHandler>();

        services.AddScoped<ITokenManagerService, TokenManagerService>();
        services.AddScoped<IHttpTokenAccessorService, HttpTokenAccessorService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IProtectedEntityManagerService, ProtectedEntityManagerService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<ISievePaginator, SievePaginator>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var assembly = Assembly.GetExecutingAssembly();

        var repositories = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository"))
            .ToList();

        foreach(var repoType in repositories)
        {
            var interfaces = repoType.GetInterfaces();

            foreach(var @interface in interfaces)
            {
                if(@interface.Name.EndsWith("Repository"))
                {
                    services.AddScoped(@interface, repoType);
                }
            }
        }

        return services;
    }
}