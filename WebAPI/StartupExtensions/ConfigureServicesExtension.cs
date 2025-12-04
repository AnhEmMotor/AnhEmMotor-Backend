using Application.DependencyInjection;
using Asp.Versioning;
using Domain.Helpers;
using Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sieve.Models;
using WebAPI.Converters;
using WebAPI.Middleware;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

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
        /// Cấu hình các dịch vụ cho ứng dụng, bao gồm tùy chọn Route, DI tầng Infrastructure, API Versioning và
        /// Swagger/OpenAPI.
        /// </summary>
        /// <param name="services">Bộ sưu tập các dịch vụ (IServiceCollection).</param>
        /// <param name="configuration">Cấu hình ứng dụng (IConfiguration).</param>
        /// <param name="environment">Môi trường web hosting (IWebHostEnvironment).</param>
        /// <returns>Bộ sưu tập các dịch vụ đã được cấu hình (IServiceCollection).</returns>
        public static IServiceCollection ConfigureServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            services.AddHttpContextAccessor();
            services.AddMapsterConfiguration(typeof(ApplicationServices).Assembly);
            services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(
                options =>
                {
                    options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                    options.SerializerOptions.Converters.Add(new EmptyStringConverter());
                });
            services.Configure<SieveOptions>(configuration.GetSection("Sieve"));
            services.AddApplicationServices();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
            var resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion);
            var otlpEndpoint = configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint");
            otlpEndpoint ??= "http://localhost:4317";
            services.AddOpenTelemetry()
                .WithTracing(
                    tracerProviderBuilder =>
                    {
                        tracerProviderBuilder.AddSource("AnhEmMotor API")
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion))
                            .AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            .AddOtlpExporter(
                                otlpOptions =>
                                {
                                    otlpOptions.Endpoint = new Uri(otlpEndpoint);
                                    otlpOptions.Protocol = OtlpExportProtocol.Grpc;
                                });
                    })
                .WithMetrics(
                    meterProviderBuilder =>
                    {
                        meterProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                            .AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            .AddRuntimeInstrumentation()
                            .AddPrometheusExporter();
                    });
            services.Configure<RouteOptions>(
                options =>
                {
                    options.LowercaseUrls = true;
                    options.LowercaseQueryStrings = true;
                });
            if(environment.IsEnvironment("Test") == false)
            {
                services.AddInfrastructureServices(configuration);
            }
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // Set true nếu chạy Production có SSL
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero, // Loại bỏ độ trễ mặc định 5 phút

                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),

                    // QUAN TRỌNG: Map claim ngắn gọn ("role") sang ClaimTypes.Role của .NET
                    RoleClaimType = "role",
                    NameClaimType = JwtRegisteredClaimNames.Name
                };
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
            services.AddControllers()
                .ConfigureApiBehaviorOptions(
                    options =>
                    {
                        options.InvalidModelStateResponseFactory = context =>
                        {
                            var errors = context.ModelState
                                .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                                .SelectMany(
                                    kvp => kvp.Value!.Errors
                                            .Select(
                                                error => new ErrorDetail
                                                        {
                                                            Field = kvp.Key,
                                                            Message = error.ErrorMessage
                                                        }))
                                .ToList();

                            var errorResponse = new
                            {
                                Title = "One or more validation errors occurred.",
                                Errors = errors
                            };

                            return new BadRequestObjectResult(errorResponse);
                        };
                    });
            services.AddSwaggerGen(
                options =>
                {
                    // Add JWT Authentication to Swagger
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT"
                    });

                    options.AddSecurityRequirement(doc =>
                    {
                        var securitySchemeReference = new OpenApiSecuritySchemeReference("Bearer", null);
                        return new OpenApiSecurityRequirement
                        {
                            [securitySchemeReference] = []
                        };
                    });

                    if(environment.IsEnvironment("Test") == false)
                    {
                        var xmlFilePath = Path.Combine(AppContext.BaseDirectory, "api.xml");
                        if(File.Exists(xmlFilePath))
                        {
                            options.IncludeXmlComments(xmlFilePath);
                        }
                    }
                });
            services.ConfigureOptions<ConfigureSwaggerOptions>();
            return services;
        }
    }
}