using Application.DependencyInjection;
using Infrastructure.DependencyInjection;
using Sieve.Models;
using WebAPI.Extensions;

namespace WebAPI.StartupExtensions;

/// <summary>
/// Provides extension methods for configuring application services in an ASP.NET Core application.
/// </summary>
/// <remarks>
/// This class contains methods that extend the IServiceCollection interface to register core framework, authentication,
/// documentation, observability, application, and infrastructure services. It is intended to be used during application
/// startup to centralize and standardize service registration.
/// </remarks>
public static class ConfigureServicesExtension
{
    private static readonly string ServiceName = "AnhEmMotor API";
    private static readonly string ServiceVersion = "1.0.0";

    /// <summary>
    /// Configures application services, including MVC, authentication, documentation, observability, application, and
    /// infrastructure layers, for the ASP.NET Core dependency injection container.
    /// </summary>
    /// <remarks>
    /// This extension method centralizes the registration of core framework, authentication, documentation,
    /// observability, application, and infrastructure services. Infrastructure services are not registered when the
    /// environment is set to "Test". Call this method during application startup to ensure all required services are
    /// available for dependency injection.
    /// </remarks>
    /// <param name="services">The service collection to which application services will be added. Must not be null.</param>
    /// <param name="configuration">The application configuration used to retrieve settings for service registration. Must not be null.</param>
    /// <param name="environment">The web hosting environment that determines environment-specific service registration. Must not be null.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance with the configured services added.</returns>
    public static IServiceCollection ConfigureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddCustomMvc();
        services.AddHttpContextAccessor();

        services.AddCustomAuthentication(configuration);

        services.AddCustomSwagger(environment);
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        services.AddCustomOpenTelemetry(configuration, ServiceName, ServiceVersion);

        services.AddApplicationServices();
        services.AddMapsterConfiguration(typeof(ApplicationServices).Assembly);
        services.Configure<SieveOptions>(configuration.GetSection("Sieve"));

        if(!environment.IsEnvironment("Test"))
        {
            services.AddInfrastructureServices(configuration);
        }

        return services;
    }
}