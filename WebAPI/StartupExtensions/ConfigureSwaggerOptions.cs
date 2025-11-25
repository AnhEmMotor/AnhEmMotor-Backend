using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebAPI.StartupExtensions
{
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
            foreach(var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    new OpenApiInfo { Title = "AnhEmMotor Web API", Version = description.ApiVersion.ToString(), });
            }
        }
    }
}