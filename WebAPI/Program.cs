using System.Globalization;
using Application;
using Asp.Versioning.ApiExplorer;
using Infrastructure;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Sieve.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using WebAPI.Extensions;
using WebAPI.Middleware;
using WebAPI.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
	try
	{
		using var httpListener = new System.Net.HttpListener();
		httpListener.Prefixes.Add("http://127.0.0.1:5000/");
		httpListener.Start();
		httpListener.Stop();
	}
	catch (System.Net.HttpListenerException)
	{
		try { System.Diagnostics.Process.GetProcessesByName("dotnet").ToList().ForEach(p => p.Kill(true)); } catch { }
		try { System.Diagnostics.Process.GetProcessesByName("WebAPI").ToList().ForEach(p => p.Kill(true)); } catch { }
		System.Threading.Thread.Sleep(500);
	}
}

builder.Host.UseSerilog();
builder.Host.ConfigureHostOptions(opts => opts.BackgroundServiceExceptionBehavior = Microsoft.Extensions.Hosting.BackgroundServiceExceptionBehavior.Ignore);
var configuration = builder.Configuration;
var environment = builder.Environment;
var customUploadPath = configuration["LocalFileStorage:UploadPath"];
if (!string.IsNullOrWhiteSpace(customUploadPath))
{
	var absolutePath = Path.IsPathRooted(customUploadPath)
		? customUploadPath
		: Path.Combine(environment.ContentRootPath, customUploadPath);
	if (!Directory.Exists(absolutePath))
	{
		Directory.CreateDirectory(absolutePath);
	}
	environment.WebRootPath = absolutePath;
	environment.WebRootFileProvider = new PhysicalFileProvider(absolutePath);
}
builder.Services.AddApplicationServices();
builder.Services.AddMemoryCache();
builder.Services.AddLocalization(options =>
options.ResourcesPath = "Resources"
);
if (!environment.IsEnvironment("Test"))
{
	builder.Services.AddInfrastructureServices(configuration);
}
builder.Services
.AddCustomLogging(configuration, "Anh Em Motor")
.AddCustomMvc()
.AddCors(
options =>
{
options.AddPolicy(
"CorsPolicy",
policy =>
{
policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000", "http://localhost:8081", "http://192.168.1.16:8081", "http://192.168.137.1:8081")
.AllowAnyMethod()
.AllowAnyHeader()
.AllowCredentials();
}
);
}
)
.AddJwtAuthentication(configuration)
.AddAuthorization()
.AddCustomSwagger(environment)
.AddCustomOpenTelemetry(configuration, "Anh Em Motor");
if (!builder.Environment.IsEnvironment("Test"))
{
	builder.Services.AddRateLimitingServices();
}
builder.Services.Configure<SieveOptions>(configuration.GetSection("Sieve"));
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
var app = builder.Build();
app.UseMiddleware<LogContextMiddleware>();
app.UseSerilogRequestLogging(
options =>
{
options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
{
var clientIp = httpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ??
httpContext.Connection.RemoteIpAddress?.ToString() ??
"unknown";
diagnosticContext.Set("ClientIP", clientIp);
};
}
);
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(
	options =>
	{
	var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
	foreach (var description in provider.ApiVersionDescriptions)
	{
		options.SwaggerEndpoint(
		$"/swagger/{description.GroupName}/swagger.json",
		description.GroupName.ToUpperInvariant()
		);
		options.EnableFilter();
	}
	options.DocExpansion(DocExpansion.None);
	}
	);
	var originalColor = Console.ForegroundColor;
	Console.ForegroundColor = ConsoleColor.Green;
	Console.WriteLine("\n=======================================================");
	Console.WriteLine(" Swagger is available at: http://localhost:5000/swagger ");
	Console.WriteLine("=======================================================\n");
	Console.ForegroundColor = originalColor;
}
app.UseStaticFiles();
app.UseRouting();

var supportedCultures = new[]
{
new CultureInfo("vi-VN") { DateTimeFormat = { ShortDatePattern = "dd/MM/yyyy" } },
new CultureInfo("en-US") { DateTimeFormat = { ShortDatePattern = "MM/dd/yyyy" } }
};
var supportedCultureNames = supportedCultures.Select(c => c.Name).ToArray();
var localizationOptions = new RequestLocalizationOptions()
.SetDefaultCulture(supportedCultures[0].Name)
.AddSupportedCultures(supportedCultureNames)
.AddSupportedUICultures(supportedCultureNames);
app.UseRequestLocalization(localizationOptions);

app.UseCors("CorsPolicy");
if (!app.Environment.IsEnvironment("Test"))
{
	app.UseForwardedHeaders();
	app.UseRateLimiter();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
if (!app.Environment.IsEnvironment("Test"))
{
	await app.ApplyMigrationsAndSeedAsync(app.Lifetime.ApplicationStopping).ConfigureAwait(false);
}
app.Run();
