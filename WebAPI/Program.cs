using Application; // Namespace mới
using Infrastructure; // Namespace mới
using Sieve.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using WebAPI.Extensions;
using WebAPI.StartupExtensions; // Vẫn giữ cho ConfigureSwaggerOptions

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment;

// =================================================================
// 1. ĐĂNG KÝ SERVICES (DEPENDENCY INJECTION)
// =================================================================

// Core Layers
builder.Services.AddApplicationServices();
if (!environment.IsEnvironment("Test"))
{
    builder.Services.AddInfrastructureServices(configuration);
}

// Web API Layer Components
builder.Services
    .AddCustomMvc()
    .AddJwtAuthentication(configuration) // Chỉ đăng ký JWT, Identity đã nằm trong Infra
    .AddCustomSwagger(environment)
    .AddCustomOpenTelemetry(configuration, "AnhEmMotor API", "1.0.0");

// Third-party configurations (nếu cần thiết để riêng)
builder.Services.Configure<SieveOptions>(configuration.GetSection("Sieve"));
builder.Services.AddHttpContextAccessor();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

// =================================================================
// 2. PIPELINE (MIDDLEWARE)
// =================================================================
var app = builder.Build();

app.UseExceptionHandler(); // Custom handler đã đăng ký trong AddCustomMvc
app.UseOpenTelemetryPrometheusScrapingEndpoint();

if (app.Environment.IsDevelopment()) // Thường Swagger chỉ bật ở Dev/Staging
{
    // Logic Swagger cũ của bạn, có thể tách ra thành app.UseCustomSwagger() extension method
    app.UseSwagger();
    app.UseSwaggerUI(
        options =>
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

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Migration & Seeding
await app.ApplyMigrationsAndSeedAsync(app.Lifetime.ApplicationStopping);

app.Run();