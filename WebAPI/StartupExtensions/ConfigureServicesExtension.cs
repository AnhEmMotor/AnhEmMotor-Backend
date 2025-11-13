using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Infrastructure.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebAPI.StartupExtensions
{
    /// <summary>
    /// Cài đặt DI cho dự án: Route luôn in thường, DB Context, bật API Versioning, tạo file api.xml để hướng dẫn route.
    /// </summary>
    public static class ConfigureServicesExtension
    {
        /// <summary>
        /// Cấu hình các dịch vụ cho ứng dụng, bao gồm tùy chọn Route, DI tầng Infrastructure, API Versioning và Swagger/OpenAPI.
        /// </summary>
        /// <param name="services">Bộ sưu tập các dịch vụ (IServiceCollection).</param>
        /// <param name="configuration">Cấu hình ứng dụng (IConfiguration).</param>
        /// <param name="environment">Môi trường web hosting (IWebHostEnvironment).</param>
        /// <returns>Bộ sưu tập các dịch vụ đã được cấu hình (IServiceCollection).</returns>
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
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

    /// <summary>
    /// Cấu hình các tài liệu Swagger cho từng phiên bản API.
    /// </summary>
    /// <param name="provider">Cung cấp mô tả về các phiên bản API đã khám phá.</param>
    public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
    {
        /// <summary>
        /// Cấu hình các tùy chọn SwaggerGen.
        /// </summary>
        /// <param name="options">Các tùy chọn cho SwaggerGen.</param>
        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Title = "Order Web API",
                    Version = description.ApiVersion.ToString(),
                });
            }
        }
    }
}