using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.LocalFile;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Authorization;
using Infrastructure.Authorization.Hander;
using Infrastructure.DBContexts;
using Infrastructure.Repositories;
using Infrastructure.Repositories.LocalFile;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Respawn;
using System.Data.Common;
using Testcontainers.PostgreSql;

namespace IntegrationTests.SetupClass;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder("postgres:17.9")
        .WithLogger(NullLogger.Instance)
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithReuse(true)
        .Build();

    private string _dbName = default!;
    private Respawner _respawner = default!;
    private DbConnection _connection = default!;
    private string _fullConnectionString = default!;

    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035
    #pragma warning disable CRR0039
    public async ValueTask InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync().ConfigureAwait(false);
        _dbName = $"Test_{Guid.NewGuid():N}";
        var baseConn = _postgreSqlContainer.GetConnectionString();
        var builder = new NpgsqlConnectionStringBuilder(baseConn) { Database = "postgres" };
        using (var masterConn = new NpgsqlConnection(builder.ConnectionString))
        {
            await masterConn.OpenAsync().ConfigureAwait(false);
            using var cmd = masterConn.CreateCommand();
            cmd.CommandText = $"CREATE DATABASE \"{_dbName}\";";
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
        builder.Database = _dbName;
        _fullConnectionString = builder.ConnectionString;
        _connection = new NpgsqlConnection(_fullConnectionString);
        await _connection.OpenAsync().ConfigureAwait(false);
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseNpgsql(_fullConnectionString)
            .Options;
        using var context = new ApplicationDBContext(options);
        await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore = ["__EFMigrationsHistory"]
            })
            .ConfigureAwait(false);
    }

    public async Task ResetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        await _respawner.ResetAsync(_connection).ConfigureAwait(false);
    }

    public new async Task DisposeAsync()
    {
        if (_connection != null)
            await _connection.DisposeAsync().ConfigureAwait(false);
    }

    #pragma warning restore CRR0039
    #pragma warning restore CRR0035
    #pragma warning restore IDE0079 

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureAppConfiguration(
            (context, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                        {
                            ["Jwt:Key"] = "ThisIsMySuperSecretAndLongEnoughKeyForJWTGenerationHehehe!@$#@#",
                            ["Jwt:Issuer"] = "https://test.api.anhemmotor.com",
                            ["Jwt:Audience"] = "https://test.anhemmotor.com",
                            ["Jwt:AccessTokenExpiryInMinutes"] = "15",
                            ["Jwt:RefreshTokenExpiryInDays"] = "7",
                            ["Cors:AllowedOrigins"] = "http://localhost:3000",
                            ["Provider"] = "PostgreSql",
                            ["ConnectionStrings:StringConnection"] = _fullConnectionString,
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
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDBContext>));
                if (dbContextDescriptor != null)
                    services.Remove(dbContextDescriptor);
                services.AddDbContext<ApplicationDBContext, PostgreSqlDbContext>(
                    (container, options) =>
                    {
                        var config = container.GetRequiredService<IConfiguration>();
                        var connectionString = config.GetConnectionString("StringConnection");
                        options.UseNpgsql(
                            connectionString,
                            npgsqlOptions =>
                            {
                                npgsqlOptions.EnableRetryOnFailure();
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
                services.Configure<AuthenticationOptions>(
                    options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    });
                services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
                services.AddScoped<IAuthorizationHandler, PermissionHandler>();
                services.AddScoped<IAuthorizationHandler, AllPermissionsHandler>();
                services.AddScoped<IAuthorizationHandler, AnyPermissionsHandler>();
                services.AddSingleton<IUserStreamService, UserStreamService>();
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
                        .FromAssemblies(typeof(DependencyInjection).Assembly)
                            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
                            .AsImplementedInterfaces()
                            .WithScopedLifetime());
            });
    }
}