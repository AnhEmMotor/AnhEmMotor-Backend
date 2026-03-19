using Application;
using Asp.Versioning.ApiExplorer;
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
    .AddCors(
        options =>
        {
            options.AddPolicy(
                "CorsPolicy",
                policy =>
                {
                    var rawOrigins = configuration["Cors:AllowedOrigins"];

                    if(string.IsNullOrWhiteSpace(rawOrigins))
                    {
                        throw new InvalidOperationException("CORS AllowedOrigins is missing in appsettings.json.");
                    }

                    var allowedOrigins = rawOrigins.Split(';', StringSplitOptions.RemoveEmptyEntries);

                    if(allowedOrigins.Any(origin => string.Compare(origin, "*") == 0))
                    {
                        throw new InvalidOperationException(
                            "Wildcard '*' is not allowed when using AllowCredentials. Please specify exact origins.");
                    }

                    policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                });
        })
    .AddJwtAuthentication(configuration)
    .AddAuthorization()
    .AddCustomSwagger(environment)
    .AddCustomOpenTelemetry(configuration, "AnhEmMotor API", "1.0.0");

if(!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddRateLimitingServices();
}
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
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach(var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
            options.DocExpansion(DocExpansion.None);
        });
    var originalColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("\n=======================================================");
    Console.WriteLine(" Swagger is available at: http://localhost:5000/swagger ");
    Console.WriteLine("=======================================================\n");
    Console.ForegroundColor = originalColor;
}

app.UseRouting();

app.UseCors("CorsPolicy");

if(!app.Environment.IsEnvironment("Test"))
{
    app.UseForwardedHeaders();
    app.UseRateLimiter();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.ApplyMigrationsAndSeedAsync(app.Lifetime.ApplicationStopping).ConfigureAwait(true);

app.Run();