using Swashbuckle.AspNetCore.SwaggerUI;
using WebAPI.Extensions;
using WebAPI.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký services (Giữ nguyên logic trong ConfigureServicesExtension của bạn)
builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Xử lý Middleware
app.UseExceptionHandler(); // Đưa lên đầu để bắt lỗi sớm
app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Swagger config
try
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
        options.DocExpansion(DocExpansion.None);
    });
}
catch (Exception ex)
{
    // Log lỗi Swagger nhưng không chết app nếu chỉ lỗi docs
    Console.WriteLine($"Swagger configuration error: {ex.Message}");
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Chạy Seeding/Migration (Code gọn gàng nhờ Extension method)
await app.ApplyMigrationsAndSeedAsync();

app.Run();