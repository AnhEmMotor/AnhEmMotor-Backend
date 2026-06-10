using Application.Common.Converters;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebAPI.Converters;
using WebAPI.Middleware;

namespace WebAPI.Extensions;

public static class MvcExtensions
{
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

        services.Configure<JsonOptions>(
            options =>
            {
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.SerializerOptions.Converters.Add(new EmptyStringConverter());
                options.SerializerOptions.Converters.Add(new NullableDecimalConverter());
            });

        services.AddControllers()
            .AddJsonOptions(
                options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.Converters.Add(new EmptyStringConverter());
                    options.JsonSerializerOptions.Converters.Add(new NullableDecimalConverter());
                });

        return services;
    }
}
