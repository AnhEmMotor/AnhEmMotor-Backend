using Asp.Versioning;
using WebAPI.Converters;
using WebAPI.Middleware;

namespace WebAPI.Extensions;

/// <summary>
/// Provides extension methods for configuring MVC services with custom conventions, API versioning, validation error
/// handling, and JSON serialization options in an ASP.NET Core application.
/// </summary>
/// <remarks>
/// This static class is intended to be used in the application's service registration phase, typically within the
/// Startup class or Program file. The provided extension methods streamline the setup of common MVC features, including
/// routing conventions, API versioning, standardized validation error responses, global exception handling, and JSON
/// serialization settings. Using these extensions helps ensure consistent behavior and reduces boilerplate code across
/// the application.
/// </remarks>
public static class MvcExtensions
{
    /// <summary>
    /// Adds and configures custom MVC, API versioning, routing, validation response, exception handling, and JSON
    /// serialization settings to the application's service collection.
    /// </summary>
    /// <remarks>
    /// This method configures routing to use lowercase URLs and query strings, sets up API versioning with URL segment
    /// support, customizes validation error responses, registers a global exception handler, and applies JSON
    /// serialization options. It is intended to be called during application startup to standardize API behavior across
    /// the application.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the MVC and related services will be added. Cannot be null.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.</returns>
    public static IServiceCollection AddCustomMvc(this IServiceCollection services)
    {
        services.Configure<RouteOptions>(
            options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });

        services.AddApiVersioning(
            config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
                config.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(
            options =>
            {
                options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                options.SerializerOptions.Converters.Add(new EmptyStringConverter());
            });

        return services;
    }
}