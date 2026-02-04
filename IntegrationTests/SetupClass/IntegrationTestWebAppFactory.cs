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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace IntegrationTests.SetupClass;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public IntegrationTestWebAppFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        // Add test configuration
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsMySuperSecretAndLongEnoughKeyForJWTGenerationHehehe!@$#@#",
                ["Jwt:Issuer"] = "https://test.api.anhemmotor.com",
                ["Jwt:Audience"] = "https://test.anhemmotor.com",
                ["Jwt:AccessTokenExpiryInMinutes"] = "15",
                ["Jwt:RefreshTokenExpiryInDays"] = "7",
                ["ConnectionStrings:StringConnection"] = "DataSource=:memory:",
                ["ProtectedAuthorizationEntities:SuperRoles:0"] = "Administrator"
            });
        });

        builder.ConfigureServices(
            services =>
            {
                // Register DbContext with SQLite for testing
                services.AddSingleton<DbConnection>(_connection);
                services.AddDbContext<ApplicationDBContext>(
                    (container, options) =>
                    {
                        var connection = container.GetRequiredService<DbConnection>();
                        options.UseSqlite(connection);
                    });

                // Register Identity (same configuration as Infrastructure)
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

                services.Configure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                });

                // Register Authorization services
                services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
                services.AddScoped<IAuthorizationHandler, PermissionHandler>();
                services.AddScoped<IAuthorizationHandler, AllPermissionsHandler>();
                services.AddScoped<IAuthorizationHandler, AnyPermissionsHandler>();

                // Register Infrastructure services
                services.AddScoped<ITokenManagerService, TokenManagerService>();
                services.AddScoped<IHttpTokenAccessorService, HttpTokenAccessorService>();
                services.AddScoped<IIdentityService, IdentityService>();
                services.AddScoped<IProtectedEntityManagerService, ProtectedEntityManagerService>();
                services.AddScoped<IProtectedProductCategoryService, ProtectedProductCategoryService>();
                services.AddScoped<IFileStorageService, LocalFileStorageService>();
                services.AddScoped<ISievePaginator, SievePaginator>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();

                // Register Repositories via Scrutor
                services.Scan(
                    scan => scan
                    .FromAssembliesOf(typeof(UnitOfWork))
                        .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
                        .AsImplementedInterfaces()
                        .WithScopedLifetime());

                // Ensure database is created
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDBContext>();
                db.Database.EnsureCreated();
            });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Close();
        _connection?.Dispose();
    }
}