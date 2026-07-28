using System.Text.RegularExpressions;
using FluentAssertions;

namespace UnitTests;

public class SidecarConfigGuard
{
    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "AnhEmMotor-Backend.sln")))
            dir = dir.Parent;
        dir.Should().NotBeNull("phải tìm được thư mục gốc chứa file .sln");
        return dir!.FullName;
    }

    [Fact(DisplayName = "GUARD_01 - Sidecar chỉ bind 127.0.0.1, không bind 0.0.0.0")]
    public void AiSidecarManager_KhongBind_TatCaInterface()
    {
        var path = Path.Combine(RepoRoot(), "Infrastructure", "Services", "Ai", "AiSidecarManager.cs");
        var content = File.ReadAllText(path);

        content.Should().NotContain("--host 0.0.0.0",
            "sidecar chỉ được gọi nội bộ");
        content.Should().Contain("--host 127.0.0.1");

        var mainPy = File.ReadAllText(Path.Combine(RepoRoot(), "AISidecar", "main.py"));
        mainPy.Should().NotContain("\"0.0.0.0\"",
            "nhánh __main__ của sidecar cũng chỉ được bind loopback");
        mainPy.Should().Contain("host=\"127.0.0.1\"");
    }

    [Fact(DisplayName = "GUARD_02 - Không commit secret thật trong appsettings.json")]
    public void Appsettings_KhongChuaSecretThat()
    {
        var webApi = Path.Combine(RepoRoot(), "WebAPI");
        var files = Directory.GetFiles(webApi, "appsettings*.json")
            .Where(f => !Path.GetFileName(f).Equals("appsettings.Development.json", StringComparison.OrdinalIgnoreCase))
            .Where(f => !Path.GetFileName(f).Equals("appsettings.Production.json", StringComparison.OrdinalIgnoreCase))
            .ToList();

        files.Should().NotBeEmpty("phải có ít nhất appsettings.Template.json trong repo");

        var realKey = new Regex(@"lsv2_pt_[0-9a-f]{32,}");
        var offenders = files
            .Where(f => realKey.IsMatch(File.ReadAllText(f)))
            .Select(Path.GetFileName)
            .ToList();

        offenders.Should().BeEmpty(
            "chuyển LangSmithApiKey sang env/user-secrets, xem mục 1.5");
    }
}
