using Application.Common.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Ai;
using Application.Interfaces.Repositories.LogisticsDashboard;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.Statistical;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Excel;
using Application.Interfaces.Services.Logistics;
using Application.Interfaces.Services.Shipping;
using Domain.Entities;
using Infrastructure.Authorization;
using Infrastructure.Authorization.Hander;
using Infrastructure.BackgroundJobs;
using Infrastructure.Configurations.Options;
using Infrastructure.DBContexts;
using Infrastructure.Repositories;
using Infrastructure.Repositories.LogisticsDashboard;
using Infrastructure.Repositories.MediaFile.File;
using Infrastructure.Repositories.Statistical;
using Infrastructure.Services;
using Infrastructure.Services.Ai;
using Infrastructure.Services.Ai.Clients;
using Infrastructure.Services.Ai.Runs;
using Infrastructure.Services.Excel;
using Infrastructure.Services.Logistics;
using Infrastructure.Services.Product;
using Infrastructure.Services.StoreChat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
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
        services.Configure<LocalFileStorageOptions>(configuration.GetSection(LocalFileStorageOptions.SectionName));
        var provider = configuration.GetValue("Provider", "SqlServer");
        if (string.Compare(provider, "MySql", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var connectionString = configuration.GetConnectionString("StringConnection") ?? string.Empty;
            var serverVersion = new MariaDbServerVersion(new Version(10, 6, 23));
            services.AddDbContextPool<ApplicationDBContext, MySqlDbContext>(
                options =>
                {
                    options.UseMySql(
                        connectionString,
                        serverVersion,
                        b => b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
                });
        } else if (string.Compare(provider, "PostgreSql", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var connectionString = configuration.GetConnectionString("StringConnection") ?? string.Empty;
            services.AddDbContextPool<ApplicationDBContext, PostgreSqlDbContext>(
                options =>
                {
                    options.UseNpgsql(
                        connectionString,
                        b => b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
                });
        } else
        {
            services.AddDbContextPool<ApplicationDBContext, SqlServerDBContext>(
                options =>
                {
                    options.UseSqlServer(
                        configuration.GetConnectionString("StringConnection"),
                        b => b.MigrationsAssembly(typeof(SqlServerDBContext).Assembly.FullName)
                                .CommandTimeout(30)
                                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    options.ConfigureWarnings(
                        w =>
                        {
                            w.Ignore(RelationalEventId.PendingModelChangesWarning);
                            w.Ignore(SqlServerEventId.SavepointsDisabledBecauseOfMARS);
                        });
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
        services.AddSingleton<IUserStreamService, UserStreamService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddScoped<IAuthorizationHandler, AllPermissionsHandler>();
        services.AddScoped<IAuthorizationHandler, AnyPermissionsHandler>();
        services.AddScoped<ITokenManagerService, TokenManagerService>();
        services.AddScoped<ICookieTokenManager, CookieTokenManager>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IProtectedEntityManagerService, ProtectedEntityManagerService>();
        services.AddScoped<IProtectedProductCategoryService, ProtectedProductCategoryService>();
        services.AddScoped<IFileReadService, FileReadService>();
        services.AddScoped<IFileInsertService, FileInsertService>();
        services.AddScoped<IStatisticalReadRepository, StatisticalReadRepository>();
        services.AddScoped<ILogisticsDashboardRepository, LogisticsDashboardRepository>();
        services.AddScoped<IFileUpdateService, FileUpdateService>();
        services.AddScoped<IFileDeleteService, FileDeleteService>();
        services.AddScoped<IExternalAuthService, ExternalAuthService>();
        services.AddScoped<IVNPayService, VNPayService>();
        services.AddScoped<IPayOSService, PayOSService>();
        services.AddScoped<ISievePaginator, SievePaginator>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBrandExcelService, BrandExcelService>();
        services.AddScoped<IProductExcelService, ProductExcelService>();
        services.AddScoped<IProductCategoryExcelService, ProductCategoryExcelService>();
        services.AddScoped<ISupplierExcelService, SupplierExcelService>();
        services.AddScoped<IPurchaseRequestExcelService, PurchaseRequestExcelService>();
        services.AddScoped<IInventoryReceiptExcelService, InventoryReceiptExcelService>();
        services.AddScoped<IInventoryLedgerExcelService, InventoryLedgerExcelService>();
        services.AddScoped<IInventoryReportExcelService, InventoryReportExcelService>();
        services.AddScoped<ISupplierDebtExcelService, SupplierDebtExcelService>();
        services.AddHostedService<OrderCleanupService>();
        services.AddHttpClient<IShippingService, ShippingService>(
            client =>
            {
                var baseAddress = configuration["GhtkSettings:BaseUrl"] ?? "https://services.ghtk.vn";
                client.BaseAddress = new Uri(baseAddress);
            });
        services.AddHttpClient<IGeocodingService, GeocodingService>();
        services.AddSingleton<IPythonEnvService, PythonEnvService>();
        services.AddSingleton<AiSidecarManager>();
        services.AddSingleton<IAiSidecarUrlProvider>(provider => provider.GetRequiredService<AiSidecarManager>());
        services.AddHostedService(provider => provider.GetRequiredService<AiSidecarManager>());
        services.AddHttpClient<AiSearchClient>();
        services.AddScoped<IAiSearchClient>(
            provider => new CachedAiSearchClient(
                provider.GetRequiredService<AiSearchClient>(),
                provider.GetRequiredService<IMemoryCache>()));
        services.AddHttpClient<IAiTestRoleClient, AiTestRoleClient>();
        services.AddSingleton<IChatRunQueue, ChatRunQueue>();
        services.AddSingleton<IProductIndexQueue, ProductIndexQueue>();
        services.AddHttpClient<ProductIndexWorker>();
        services.AddHostedService(provider => provider.GetRequiredService<ProductIndexWorker>());
        services.AddSingleton<IChatRunEventBus, ChatRunEventBus>();
        services.AddSingleton<IChatRunCancellationRegistry, ChatRunCancellationRegistry>();
        services.AddSingleton<IChatRunTokenStore, ChatRunTokenStore>();
        services.AddSingleton<IChatToolCatalogProvider, ChatToolCatalogProvider>();
        services.AddSingleton<IServerDateProvider, SystemServerDateProvider>();
        services.AddScoped<IChatRunWriter, ChatRunWriter>();
        services.AddScoped<ISidecarStreamClient, SidecarStreamClient>();
        services.AddScoped<IStoreChatAiClient, StoreChatAiClient>();
        // services.AddHostedService<ChatRunExecutor>();
        // services.AddHostedService<OrphanedRunCleaner>();
        // services.AddHostedService<ChatRunEventCleanupJob>();
        // services.AddHostedService<ProductViewCleanupJob>();
        // services.AddHostedService<StaleWaitingSessionMonitor>();
        services.Scan(
            scan => scan
                .FromAssemblies(Assembly.GetExecutingAssembly())
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        services.AddHostedService<SupplierContractExpiryWorker>();
        return services;
    }
}
