using Application;
using Infrastructure;
using Sieve.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using WebAPI.Extensions;
using WebAPI.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment;

builder.Services.AddApplicationServices();
if(!environment.IsEnvironment("Test"))
{
    builder.Services.AddInfrastructureServices(configuration);
}

builder.Services
    .AddCustomMvc()
    .AddJwtAuthentication(configuration)
    .AddCustomSwagger(environment)
    .AddCustomOpenTelemetry(configuration, "AnhEmMotor API", "1.0.0");

builder.Services.Configure<SieveOptions>(configuration.GetSection("Sieve"));
builder.Services.AddHttpContextAccessor();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseOpenTelemetryPrometheusScrapingEndpoint();

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        options =>
        {
            var provider = app.Services.GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
            foreach(var description in provider.ApiVersionDescriptions)
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

await app.ApplyMigrationsAndSeedAsync(app.Lifetime.ApplicationStopping).ConfigureAwait(true);

app.Run();