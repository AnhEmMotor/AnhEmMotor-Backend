using Asp.Versioning;
using Infrastructure.DependencyInjection;

namespace WebAPI.StartupExtensions
{
    public static class ConfigureServicesExtension
    {
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
                    var xmlApiPath = Path.Combine(AppContext.BaseDirectory, "api.xml");
                    if (File.Exists(xmlApiPath))
                    {
                        options.IncludeXmlComments(xmlApiPath);
                    }
                }
            });
            services.ConfigureOptions<ConfigureSwaggerOptions>();
            services.AddControllers();
            return services;
        }
    }
}