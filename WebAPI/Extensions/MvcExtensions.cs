using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Converters;
using WebAPI.Middleware;

namespace WebAPI.StartupExtensions;

/// <summary>
/// Provides extension methods for configuring MVC services with custom conventions, API versioning, validation error
/// handling, and JSON serialization options in an ASP.NET Core application.
/// </summary>
/// <remarks>This static class is intended to be used in the application's service registration phase, typically
/// within the Startup class or Program file. The provided extension methods streamline the setup of common MVC
/// features, including routing conventions, API versioning, standardized validation error responses, global exception
/// handling, and JSON serialization settings. Using these extensions helps ensure consistent behavior and reduces
/// boilerplate code across the application.</remarks>
public static class MvcExtensions
{
    /// <summary>
    /// Adds and configures custom MVC, API versioning, routing, validation response, exception handling, and JSON
    /// serialization settings to the application's service collection.
    /// </summary>
    /// <remarks>This method configures routing to use lowercase URLs and query strings, sets up API
    /// versioning with URL segment support, customizes validation error responses, registers a global exception
    /// handler, and applies JSON serialization options. It is intended to be called during application startup to
    /// standardize API behavior across the application.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the MVC and related services will be added. Cannot be null.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.</returns>
    public static IServiceCollection AddCustomMvc(this IServiceCollection services)
    {
        // 1. Routing
        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });

        // 2. Versioning
        services.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;
            config.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // 3. Controllers & Validation Response
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                        .SelectMany(kvp => kvp.Value!.Errors.Select(error => new Application.Common.Models.ErrorDetail
                        {
                            Field = kvp.Key,
                            Message = error.ErrorMessage
                        }))
                        .ToList();

                    return new BadRequestObjectResult(new
                    {
                        Title = "One or more validation errors occurred.",
                        Errors = errors
                    });
                };
            });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // JSON Options
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.Converters.Add(new EmptyStringConverter()); // Uncomment nếu cần
        });

        return services;
    }
}