using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebAPI.StartupExtensions
{
    /// <summary>
    /// C?u hình các tài li?u Swagger cho t?ng phiên b?n API.
    /// </summary>
    /// <param name="provider">Cung c?p mô t? v? các phiên b?n API dã khám phá.</param>
    public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
    {
        /// <summary>
        /// C?u hình các tùy ch?n SwaggerGen.
        /// </summary>
        /// <param name="options">Các tùy ch?n cho SwaggerGen.</param>
        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    new OpenApiInfo { Title = "AnhEmMotor Web API", Version = description.ApiVersion.ToString(), });
            }
        }
    }
}
