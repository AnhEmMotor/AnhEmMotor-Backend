using Swashbuckle.AspNetCore.SwaggerUI;
using WebAPI.Extensions;
using WebAPI.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseExceptionHandler();
app.UseOpenTelemetryPrometheusScrapingEndpoint();

try
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
} catch(Exception ex)
{
    Console.WriteLine($"Swagger configuration error: {ex.Message}");
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var cancellationToken = app.Lifetime.ApplicationStopping;
await app.ApplyMigrationsAndSeedAsync(cancellationToken).ConfigureAwait(true);

app.Run();