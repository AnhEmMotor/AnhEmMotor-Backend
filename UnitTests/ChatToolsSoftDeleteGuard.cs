using FluentAssertions;

namespace UnitTests;

public class ChatToolsSoftDeleteGuard
{
    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "AnhEmMotor-Backend.sln")))
            dir = dir.Parent;
        dir.Should().NotBeNull("phải tìm được thư mục gốc chứa file .sln");
        return dir!.FullName;
    }

    [Fact(DisplayName = "GUARD_10 - Tool chat không được vượt qua soft-delete filter (Stage 16.2 mục #1)")]
    public void ChatTools_KhongDung_IgnoreQueryFilters()
    {
        var dir = Path.Combine(RepoRoot(), "Application", "Features", "ChatTools");
        var offenders = Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories)
            .Where(
                f =>
                {
                    var content = File.ReadAllText(f);
                    return content.Contains("IgnoreQueryFilters") ||
                        content.Contains(".All<") ||
                        content.Contains("DataFetchMode.All") ||
                        content.Contains("DataFetchMode.DeletedOnly");
                })
            .Select(f => Path.GetRelativePath(RepoRoot(), f))
            .ToList();
        offenders.Should().BeEmpty("tool chat phải tôn trọng global query filter DeletedAt == null, xem mục 16.2");
    }
}
