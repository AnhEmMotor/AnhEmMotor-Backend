using Domain.Entities;
using Infrastructure.DBContexts;
using Infrastructure.Seeders;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.SwaggerUI;
using WebAPI.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

var app = builder.Build();

var hostLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var cancellationToken = hostLifetime.ApplicationStopping;

using(var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var dbContext = serviceProvider.GetRequiredService<ApplicationDBContext>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var shouldSeed = configuration.GetValue<bool>("SeedingOptions:RunDataSeedingOnStartup");

    if(shouldSeed)
    {
        await PermissionDataSeeder.SeedPermissionsAsync(dbContext, cancellationToken).ConfigureAwait(false);
        await ProtectedEntitiesSeeder.SeedProtectedEntitiesAsync(
            dbContext,
            roleManager,
            userManager,
            configuration,
            cancellationToken)
            .ConfigureAwait(false);
    }
}

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
} catch(System.Reflection.ReflectionTypeLoadException ex)
{
    foreach(var loaderException in ex.LoaderExceptions)
    {
        Console.WriteLine(loaderException?.Message);
    }
    throw;
}

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();
app.MapControllers();
app.Run();