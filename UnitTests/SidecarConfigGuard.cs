using FluentAssertions;
using System.Text.RegularExpressions;

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
        content.Should().NotContain("--host 0.0.0.0", "sidecar chỉ được gọi nội bộ");
        content.Should().Contain("--host 127.0.0.1");
        var mainPy = File.ReadAllText(Path.Combine(RepoRoot(), "AISidecar", "main.py"));
        mainPy.Should().NotContain("\"0.0.0.0\"", "nhánh __main__ của sidecar cũng chỉ được bind loopback");
        mainPy.Should().Contain("host=\"127.0.0.1\"");
    }

    [Fact(DisplayName = "GUARD_03 - AiSidecarManager trỏ đúng entrypoint app.main:app")]
    public void AiSidecarManager_TroDung_Entrypoint()
    {
        var path = Path.Combine(RepoRoot(), "Infrastructure", "Services", "Ai", "AiSidecarManager.cs");
        var content = File.ReadAllText(path);
        content.Should().Contain("app.main:app", "sau Stage 7, entrypoint là app/main.py chứ không phải main.py ở gốc");
        content.Should().NotContain("uvicorn main:app");
    }
}
