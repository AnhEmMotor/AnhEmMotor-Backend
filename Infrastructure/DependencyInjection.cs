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
using System.Reflection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Database Context
        services.AddDbContextPool<ApplicationDBContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("StringConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDBContext).Assembly.FullName).CommandTimeout(30));
        });

        // 2. Identity Core (Kết nối với DB)
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Cấu hình password rule tại đây
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDBContext>()
        .AddDefaultTokenProviders();

        // 3. Authorization Handlers & Policy
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddScoped<IAuthorizationHandler, AllPermissionsHandler>();
        services.AddScoped<IAuthorizationHandler, AnyPermissionsHandler>();

        // 4. Services Implementation
        services.AddScoped<ITokenManagerService, TokenManagerService>();
        services.AddScoped<IHttpTokenAccessorService, HttpTokenAccessorService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IProtectedEntityManagerService, ProtectedEntityManagerService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<ISievePaginator, SievePaginator>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 5. Auto-register Repositories (Viết lại cho gọn)
        // Cách này quét tất cả class có tên kết thúc bằng Repository và implement Interface tương ứng
        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // LƯU Ý: Đoạn code trên sử dụng thư viện "Scrutor".
        // Nếu bạn chưa cài, hãy chạy: dotnet add package Scrutor
        // Nếu không muốn dùng Scrutor, hãy giữ lại logic vòng lặp cũ của bạn nhưng gói gọn vào hàm riêng.

        return services;
    }
}