using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data.Common;

//using OrderWebAPI.Infrastructure.DBContexts;
using System.Security.Claims;

namespace IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registrations
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<ApplicationDBContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // Remove all DbContextOptions to avoid conflicts
                var dbContextOptionsDescriptors = services.Where(
                    d => d.ServiceType.IsGenericType &&
                         d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
                    .ToList();

                foreach (var descriptor in dbContextOptionsDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory Database (this is registered AFTER removing SQL Server)
                //services.AddDbContext<ApplicationDBContext>(options =>
                //{
                //    options.UseInMemoryDatabase("InMemoryTestDb");
                //});
                services.AddSingleton<DbConnection>(container =>
                {
                    var connection = new SqliteConnection("DataSource=:memory:");
                    connection.Open();
                    return connection;
                });

                // Remove existing authorization handlers and policies
                services.RemoveAll<IAuthorizationHandler>();
                services.RemoveAll<IAuthorizationPolicyProvider>();

                // Add test authentication scheme
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

                // Add test policy provider that always allows access
                services.AddSingleton<IAuthorizationPolicyProvider, TestPolicyProvider>();
                services.AddSingleton<IAuthorizationHandler, TestAuthorizationHandler>();
            });

            // Configure the application to skip Permission seeding
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["SkipPermissionSeeding"] = "true"
                });
            });
        }
    }

    /// <summary>
    /// Test policy provider that creates policies for any requested policy name
    /// </summary>
    public class TestPolicyProvider : IAuthorizationPolicyProvider
    {
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return Task.FromResult(new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build());
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return Task.FromResult<AuthorizationPolicy?>(new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build());
        }

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Create a policy that always succeeds for any policy name
            var policy = new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }
    }

    /// <summary>
    /// Test authorization handler that always succeeds
    /// </summary>
    public class TestAuthorizationHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (var requirement in context.PendingRequirements.ToList())
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Test authentication handler that always succeeds
    /// </summary>
    public class TestAuthHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
