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
using MySqlConnector;
using Respawn;
using System.Data.Common;
using Testcontainers.MySql;

namespace IntegrationTests.SetupClass;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly MySqlContainer _mySqlContainer = new MySqlBuilder("mysql:8.0")
        .WithLogger(NullLogger.Instance)
        .WithUsername("root")
        .WithPassword("root")
        .WithReuse(true)
        .WithCommand("--innodb_flush_log_at_trx_commit=2", "--sync_binlog=0", "--innodb_use_native_aio=0")
        .Build();

    private string _dbName = default!;
    private Respawner _respawner = default!;
    private DbConnection _connection = default!;

#pragma warning disable IDE0079 
#pragma warning disable CRR0035
#pragma warning disable CRR0039
    public async ValueTask InitializeAsync()
    {
        await _mySqlContainer.StartAsync().ConfigureAwait(false);

        _dbName = $"AnhEmMotor_Test_{Guid.NewGuid():N}";
        var baseConnectionString = _mySqlContainer.GetConnectionString();
        var builder = new MySqlConnectionStringBuilder(baseConnectionString) { Database = string.Empty };

        using(var masterConnection = new MySqlConnection(builder.ConnectionString))
        {
            await masterConnection.OpenAsync().ConfigureAwait(false);
            using var command = masterConnection.CreateCommand();
            command.CommandText = $"CREATE DATABASE `{_dbName}`;";
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        builder.Database = _dbName;
        var connectionString = builder.ConnectionString;

        _connection = new MySqlConnection(connectionString);
        await _connection.OpenAsync().ConfigureAwait(false);

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDBContext>();
        optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));

        using var tempDbContext = new ApplicationDBContext(optionsBuilder.Options);
        await tempDbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);

        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.MySql,
                SchemasToInclude = [ _dbName ],
                TablesToIgnore = [ "__EFMigrationsHistory" ]
            })
            .ConfigureAwait(false);
    }


    public async Task ResetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _respawner.ResetAsync(_connection).ConfigureAwait(false);
    }

    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync().ConfigureAwait(false);

        var baseConnectionString = _mySqlContainer.GetConnectionString();
        var builder = new MySqlConnectionStringBuilder(baseConnectionString) { Database = string.Empty };

        using(var masterConnection = new MySqlConnection(builder.ConnectionString))
        {
            await masterConnection.OpenAsync().ConfigureAwait(false);
            using var command = masterConnection.CreateCommand();
            command.CommandText = $"DROP DATABASE IF EXISTS `{_dbName}`;";
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
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
                var connectionString = _mySqlContainer.GetConnectionString();
                var connBuilder = new MySqlConnectionStringBuilder(connectionString) { Database = _dbName };
                var connString = connBuilder.ConnectionString;

                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                        {
                            ["Jwt:Key"] = "ThisIsMySuperSecretAndLongEnoughKeyForJWTGenerationHehehe!@$#@#",
                            ["Jwt:Issuer"] = "https://test.api.anhemmotor.com",
                            ["Jwt:Audience"] = "https://test.anhemmotor.com",
                            ["Jwt:AccessTokenExpiryInMinutes"] = "15",
                            ["Jwt:RefreshTokenExpiryInDays"] = "7",
                            ["Cors:AllowedOrigins"] = "http://localhost:3000",
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
                .FromAssembliesOf(typeof(UnitOfWork))
                            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
                            .AsImplementedInterfaces()
                            .WithScopedLifetime());
            });
    }
}