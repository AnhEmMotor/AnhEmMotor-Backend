using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebAPI.StartupExtensions;

/// <summary>
/// Provides extension methods for configuring custom Swagger/OpenAPI services in an ASP.NET Core application.
/// </summary>
/// <remarks>This class contains static methods that extend the service collection to add Swagger generation and
/// configuration, including JWT bearer authentication support and XML documentation integration. Use these extensions
/// during application startup to enable API documentation and security definitions for your web APIs.</remarks>
public static class SwaggerExtensions
{
    /// <summary>
    /// Adds and configures Swagger services for API documentation, including JWT bearer authentication support and XML
    /// comments integration based on the hosting environment.
    /// </summary>
    /// <remarks>XML comments are included in the Swagger documentation only if the environment is not set to
    /// "Test" and the XML file exists. JWT bearer authentication is configured for API security. This method enables
    /// Swagger annotations and registers additional Swagger options.</remarks>
    /// <param name="services">The service collection to which Swagger services will be added.</param>
    /// <param name="environment">The current web hosting environment, used to determine whether to include XML comments in the Swagger
    /// configuration.</param>
    /// <returns>The same service collection instance, allowing for method chaining.</returns>
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddSwaggerGen(
                options =>
                {
                    options.AddSecurityDefinition(
                        "bearer",
                        new OpenApiSecurityScheme
                        {
                            Description =
                                    "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                            Name = "Authorization",
                            In = ParameterLocation.Header,
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer",
                            BearerFormat = "JWT",
                        });
                    options.AddSecurityRequirement(
                        document => new OpenApiSecurityRequirement
                        {
                            [new OpenApiSecuritySchemeReference("bearer", document)] = []
                        });
                    if (environment.IsEnvironment("Test") == false)
                    {
                        var xmlFilePath = Path.Combine(AppContext.BaseDirectory, "api.xml");
                        if (File.Exists(xmlFilePath))
                        {
                            options.IncludeXmlComments(xmlFilePath);
                        }
                    }
                    options.EnableAnnotations();
                });

        // Đăng ký Options (để tách class ConfigureSwaggerOptions ra nếu cần, hoặc giữ nguyên logic cũ)

        return services;
    }
}