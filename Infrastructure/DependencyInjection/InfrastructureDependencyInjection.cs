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
// Import các namespace cần thiết của dự án bạn...

namespace Infrastructure.DependencyInjection; // Namespace ví dụ

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Database Configuration
        // Dùng AddDbContextPool để tối ưu hiệu năng thay vì AddDbContext thường
        services.AddDbContextPool<ApplicationDBContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("StringConnection"),
                b =>
                {
                    b.MigrationsAssembly("Infrastructure");
                    b.CommandTimeout(30); // XÓA NGAY. Để mặc định hoặc set ít nhất 30s.
                });
        });

        // 2. Auth & Core Services
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

        // UnitOfWork nên được đăng ký rõ ràng
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 3. AUTO-REGISTER REPOSITORIES (Sự thay đổi quan trọng nhất)
        // Chiến thuật: Tìm tất cả các class trong Assembly này (Infrastructure),
        // có tên kết thúc bằng "Repository" và là class (không phải abstract/interface).
        // Sau đó đăng ký nó với Interface tương ứng.

        var assembly = Assembly.GetExecutingAssembly();

        var repositories = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository"))
            .ToList();

        foreach (var repoType in repositories)
        {
            // Tìm tất cả các interface mà class này implement
            // Loại trừ các interface hệ thống nếu cần, nhưng thường thì lấy hết interface trực tiếp là ổn.
            var interfaces = repoType.GetInterfaces();

            foreach (var @interface in interfaces)
            {
                // Chỉ đăng ký nếu interface cũng có tên Repository (để tránh đăng ký nhầm IDisposable v.v.)
                // Hoặc bạn có thể tạo 1 interface marker IRepository để an toàn hơn.
                if (@interface.Name.EndsWith("Repository"))
                {
                    services.AddScoped(@interface, repoType);
                }
            }
        }

        // Services liên quan Mapster vẫn giữ nguyên hoặc tách riêng tùy bạn
        // Nhưng logic MapsterInstaller nên gọi ở layer WebAPI hoặc Application thì đúng hơn
        // services.AddMapsterConfiguration... (Tôi thấy bạn để ở file riêng, giữ nguyên cũng được)

        return services;
    }
}