using FluentAssertions;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace UnitTests;

public class CleanArchitectureGuard
{
    private static readonly string[] ForbiddenApplicationPackages =
    [
        "Asp.Versioning.Mvc",
        "Asp.Versioning.Mvc.ApiExplorer",
        "Microsoft.AspNetCore.Authentication.JwtBearer",
        "Swashbuckle.AspNetCore",
        "Swashbuckle.AspNetCore.Annotations",
        "Serilog.AspNetCore",
        "Serilog.Enrichers.Process",
        "Serilog.Enrichers.Thread",
        "Serilog.Sinks.OpenTelemetry",
        "OpenTelemetry.Exporter.OpenTelemetryProtocol",
        "OpenTelemetry.Exporter.Prometheus.AspNetCore",
        "OpenTelemetry.Extensions.Hosting",
        "OpenTelemetry.Instrumentation.AspNetCore",
        "OpenTelemetry.Instrumentation.Http",
        "OpenTelemetry.Instrumentation.Runtime",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.EntityFrameworkCore.Design",
        "Microsoft.Extensions.Identity.Stores",
        "ClosedXML",
    ];

    [Fact]
    public void Application_Csproj_Does_Not_Reference_Infrastructure_Packages()
    {
        var csprojPath = GetApplicationCsprojPath();
        var referenced = XDocument.Load(csprojPath).Descendants("PackageReference")
            .Select(e => e.Attribute("Include")?.Value)
            .Where(name => name is not null)
            .ToList();

        referenced.Should().NotContain(ForbiddenApplicationPackages);
    }

    private static string GetApplicationCsprojPath([CallerFilePath] string testFilePath = "")
    {
        var unitTestsDir = Path.GetDirectoryName(testFilePath)!;
        var repoRoot = Path.GetDirectoryName(unitTestsDir)!;
        return Path.Combine(repoRoot, "Application", "Application.csproj");
    }
}
