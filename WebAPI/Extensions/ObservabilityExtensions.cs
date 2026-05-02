using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;

namespace WebAPI.Extensions;

/// <summary>
/// Provides extension methods for configuring OpenTelemetry tracing and metrics with custom service information in an
/// ASP.NET Core application.
/// </summary>
/// <remarks>
/// This static class contains extension methods that simplify the setup of OpenTelemetry instrumentation and exporting
/// for services. The extensions are intended to be used during application startup to enable distributed tracing and
/// metrics collection with minimal configuration. All methods are thread-safe and designed for use in dependency
/// injection scenarios.
/// </remarks>
public static class ObservabilityExtensions
{
    /// <summary>
    /// Adds custom logging using Serilog and Grafana Loki to the service collection.
    /// </summary>
    /// <param name="services">The service collection to which logging services will be added.</param>
    /// <param name="configuration">The application configuration used to retrieve Loki settings.</param>
    /// <param name="serviceName">The name of the service to be used for logging identification.</param>
    /// <returns>The same service collection instance with Serilog configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services or configuration is null.</exception>
    public static IServiceCollection AddCustomLogging(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        var otelUrl = configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint") ?? "http://localhost:4317";
        Log.Logger = new LoggerConfiguration()
            .ReadFrom
            .Configuration(configuration)
            .Enrich
            .FromLogContext()
            .Enrich
            .WithProcessId()
            .Enrich
            .WithThreadId()
            .WriteTo
            .Logger(
                lc => lc
                .Filter
                    .ByIncludingOnly(
                        le => le.Level >= LogEventLevel.Warning ||
                                le.Properties.ContainsKey("StatusCode") ||
                                le.MessageTemplate.Text.Contains("OpenTelemetry"))
                    .WriteTo
                    .OpenTelemetry(
                        options =>
                        {
                            options.Endpoint = otelUrl;
                            options.Protocol = OtlpProtocol.Grpc;
                            options.ResourceAttributes = new Dictionary<string, object>
                            {
                                ["service.name"] = "Anh Em Motor",
                                ["deployment.environment"] =
                                    configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") ?? "Production"
                            };
                        }))
            .WriteTo
            .Logger(
                lc => lc
                .Filter
                    .ByIncludingOnly(le => !le.Properties.ContainsKey("StatusCode"))
                    .WriteTo
                    .Console())
            .CreateLogger();
        return services;
    }

    /// <summary>
    /// Adds OpenTelemetry tracing and metrics to the service collection using the specified configuration, service
    /// name, and service version.
    /// </summary>
    /// <remarks>
    /// This method configures both tracing and metrics for ASP.NET Core and HTTP client instrumentation, sets up
    /// resource information, and exports telemetry data using OTLP and Prometheus exporters. The OTLP endpoint is read
    /// from the configuration key 'OpenTelemetry:OtlpEndpoint'; if not specified, it defaults to
    /// 'http://localhost:4317'.
    /// </remarks>
    /// <param name="services">The service collection to which OpenTelemetry services will be added.</param>
    /// <param name="configuration">The application configuration used to retrieve OpenTelemetry settings, such as the OTLP endpoint.</param>
    /// <param name="serviceName">The name of the service to be used for OpenTelemetry resource identification and instrumentation.</param>
    /// <param name="serviceVersion">The version of the service to be used for OpenTelemetry resource identification.</param>
    /// <returns>The same service collection instance with OpenTelemetry tracing and metrics configured.</returns>
    public static IServiceCollection AddCustomOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        string? serviceVersion = null)
    {
        var otlpEndpoint = configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint") ?? "http://localhost:4317";
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion);
        services.AddOpenTelemetry()
            .WithTracing(
                tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                    .AddSource(serviceName)
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter(
                            otlpOptions =>
                            {
                                otlpOptions.Endpoint = new Uri(otlpEndpoint);
                                otlpOptions.Protocol = OtlpExportProtocol.Grpc;
                            });
                })
            .WithMetrics(
                meterProviderBuilder =>
                {
                    meterProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddOtlpExporter(
                            (otlpOptions, metricReaderOptions) =>
                            {
                                otlpOptions.Endpoint = new Uri(otlpEndpoint);
                                otlpOptions.Protocol = OtlpExportProtocol.Grpc;
                                metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 5000;
                            });
                });
        return services;
    }
}
