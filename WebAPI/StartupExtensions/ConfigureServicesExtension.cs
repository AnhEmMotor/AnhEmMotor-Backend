using Application.DependencyInjection;
using Asp.Versioning;
using Domain.Helpers;
using Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sieve.Models;
using WebAPI.Middleware;

namespace WebAPI.StartupExtensions
{
    /// <summary>
    /// Cài đặt DI cho dự án: Route luôn in thường, DB Context, bật API Versioning, tạo file api.xml để hướng dẫn route.
    /// </summary>
    public static class ConfigureServicesExtension
    {
        private static readonly string serviceName = "AnhEmMotor API";
        private static readonly string serviceVersion = "1.0.0";
        /// <summary>
        /// Cấu hình các dịch vụ cho ứng dụng, bao gồm tùy chọn Route, DI tầng Infrastructure, API Versioning và Swagger/OpenAPI.
        /// </summary>
        /// <param name="services">Bộ sưu tập các dịch vụ (IServiceCollection).</param>
        /// <param name="configuration">Cấu hình ứng dụng (IConfiguration).</param>
        /// <param name="environment">Môi trường web hosting (IWebHostEnvironment).</param>
        /// <returns>Bộ sưu tập các dịch vụ đã được cấu hình (IServiceCollection).</returns>
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddApplicationServices();
            services.Configure<SieveOptions>(configuration.GetSection("Sieve"));
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
            var resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion);
            var otlpEndpoint = configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint");
            otlpEndpoint ??= "http://localhost:4317";
            services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder.AddSource("AnhEmMotor API")
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(
                            serviceName,
                            serviceVersion))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(otlpEndpoint);
                    otlpOptions.Protocol = OtlpExportProtocol.Grpc;
                });
            }).WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();
            });
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });
            if (environment.IsEnvironment("Test") == false)
            {
                services.AddInfrastructureServices(configuration);
            }
            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
                config.ApiVersionReader = new UrlSegmentApiVersionReader();
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
            services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                        .SelectMany(kvp =>
                            kvp.Value!.Errors.Select(error => new ErrorDetail
                            {
                                Field = kvp.Key,
                                Message = error.ErrorMessage
                            })
                        )
                        .ToList();

                    var errorResponse = new
                    {
                        Title = "One or more validation errors occurred.",
                        Errors = errors
                    };

                    return new BadRequestObjectResult(errorResponse);
                };
            });
            services.AddSwaggerGen(options =>
            {
                if (environment.IsEnvironment("Test") == false)
                {
                    var xmlFilePath = Path.Combine(AppContext.BaseDirectory, "api.xml");
                    if (File.Exists(xmlFilePath))
                    {
                        options.IncludeXmlComments(xmlFilePath);
                    }
                }
            });
            services.ConfigureOptions<ConfigureSwaggerOptions>();
            services.AddControllers();
            return services;
        }
    }
}