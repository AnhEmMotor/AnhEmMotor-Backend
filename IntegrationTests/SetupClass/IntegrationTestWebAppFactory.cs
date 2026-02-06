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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Respawn;
using System.Data.Common;
using Testcontainers.MySql;

namespace IntegrationTests.SetupClass;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MySqlContainer? _mySqlContainer;
    private readonly bool _isRunningInCI;
    private readonly string? _ciConnectionString;

    public IntegrationTestWebAppFactory()
    {
        _isRunningInCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
                         !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

        if (_isRunningInCI)
        {
            _ciConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__StringConnection")
                ?? "Server=127.0.0.1;Port=3306;Database=AnhEmMotor_Test;User=root;Password=root;";
        }
        else
        {
            _mySqlContainer = new MySqlBuilder("mysql:8.0")
                .WithDatabase("AnhEmMotor_Test")
                .WithUsername("root")
                .WithPassword("root")
                .Build();
            
        }
    }

    private Respawner _respawner = default!;
    private DbConnection _connection = default!;

#pragma warning disable IDE0079 
#pragma warning disable CRR0039
    public async Task InitializeAsync()
    {
        string connectionString;

        if (_isRunningInCI)
        {
            connectionString = _ciConnectionString!;
        }
        else
        {
            await _mySqlContainer!.StartAsync().ConfigureAwait(false);
            connectionString = _mySqlContainer.GetConnectionString();
        }

        _connection = new MySqlConnection(connectionString);
        await _connection.OpenAsync().ConfigureAwait(false);

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDBContext>();
        optionsBuilder.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 0)));

        using var tempDbContext = new ApplicationDBContext(optionsBuilder.Options);
        await tempDbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);

        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.MySql,
                SchemasToInclude = [ "AnhEmMotor_Test" ],
                TablesToIgnore = [ "__EFMigrationsHistory" ]
            })
            .ConfigureAwait(false);
    }

#pragma warning restore CRR0039
#pragma warning restore IDE0079 

    public async Task ResetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _respawner.ResetAsync(_connection).ConfigureAwait(false);
    }

    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync().ConfigureAwait(false);
        
        // Only stop Testcontainers if running locally
        if (!_isRunningInCI && _mySqlContainer is not null)
        {
            await _mySqlContainer.StopAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration(
            (context, config) =>
            {
                var connString = _isRunningInCI 
                    ? _ciConnectionString! 
                    : _mySqlContainer!.GetConnectionString();

                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                        {
                            ["Jwt:Key"] = "ThisIsMySuperSecretAndLongEnoughKeyForJWTGenerationHehehe!@$#@#",
                            ["Jwt:Issuer"] = "https://test.api.anhemmotor.com",
                            ["Jwt:Audience"] = "https://test.anhemmotor.com",
                            ["Jwt:AccessTokenExpiryInMinutes"] = "15",
                            ["Jwt:RefreshTokenExpiryInDays"] = "7",
                            ["ConnectionStrings:StringConnection"] = connString,
                            ["ProtectedAuthorizationEntities:SuperRoles:0"] = "Administrator",
                            ["Logging:LogLevel:Default"] = "Warning",
                            ["Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command"] = "None",
                            ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Warning",
                            ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning",
                            ["Logging:LogLevel:LuckyPennySoftware.MediatR.License"] = "None"
                        });

                config.AddEnvironmentVariables();
            });

        builder.ConfigureServices(
            services =>
            {
                var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));
                if(dbConnectionDescriptor != null)
                    services.Remove(dbConnectionDescriptor);

                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDBContext>));
                if(dbContextDescriptor != null)
                    services.Remove(dbContextDescriptor);

                services.AddDbContext<ApplicationDBContext>(
                    (container, options) =>
                    {
                        var config = container.GetRequiredService<IConfiguration>();
                        var connectionString = config.GetConnectionString("StringConnection");

                        options.UseMySql(
                            connectionString,
                            new MySqlServerVersion(new Version(8, 0, 0)),
                            mySqlOptions =>
                            {
                                mySqlOptions.EnableRetryOnFailure();
                            });
                    });

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

                services.Configure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(
                    options =>
                    {
                        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                    });

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
                .FromAssembliesOf(typeof(UnitOfWork))
                            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
                            .AsImplementedInterfaces()
                            .WithScopedLifetime());
            });
    }
}