using WebAPI.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
    }
});

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseRouting();
app.UseExceptionHandler();
app.MapControllers();
app.Run();

/// <summary>
/// Đầu vào của chương trình! Bạn không cần quan tâm đến đây!
/// </summary>
public partial class Program { }