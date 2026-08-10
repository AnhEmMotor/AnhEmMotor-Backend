using FluentAssertions;

namespace UnitTests;

public class GlossaryGuard
{
    private static readonly string[] RequiredHeadings = ["## Doanh thu", "## Số đơn hàng", "## Lợi nhuận", "## Tồn kho", "## Khách hàng mới", "## \"Tháng này\""];

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "AnhEmMotor-Backend.sln")))
            dir = dir.Parent;
        dir.Should().NotBeNull("phải tìm được thư mục gốc chứa file .sln");
        return dir!.FullName;
    }

    [Fact(DisplayName = "GLOSSARY_01 - docs/chatbot-ai/GLOSSARY.md tồn tại và đủ 6 khái niệm bắt buộc (Stage 16.4)")]
    public void Glossary_TonTai_VaDuKhaiNiemBatBuoc()
    {
        var path = Path.Combine(RepoRoot(), "docs", "chatbot-ai", "GLOSSARY.md");
        File.Exists(path).Should().BeTrue("GLOSSARY.md phải tồn tại trước khi viết tool tài chính, xem Stage 16.4");
        var content = File.ReadAllText(path);
        foreach (var heading in RequiredHeadings)
        {
            content.Should().Contain(heading, $"GLOSSARY.md phải có mục {heading}");
        }
    }
}
