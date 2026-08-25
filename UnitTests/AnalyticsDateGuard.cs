using FluentAssertions;

namespace UnitTests;

public class AnalyticsDateGuard
{
    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AnhEmMotor-Backend.sln")))
            directory = directory.Parent;
        directory.Should().NotBeNull("phải tìm được thư mục gốc chứa file .sln");
        return directory!.FullName;
    }

    [Fact(DisplayName = "ANALYTICS_DATE_01 - Ngày báo cáo nhân sự không bị dịch ngày khi đổi UTC")]
    public void StaffReportDate_PreservesCalendarDate()
    {
        var path = Path.Combine(RepoRoot(), "WebAPI", "Controllers", "AnalyticsController.cs");
        var content = File.ReadAllText(path);
        var methodStart = content.IndexOf("public async Task<IActionResult> GetStaff", StringComparison.Ordinal);
        var methodEnd = content.IndexOf("Gets the most recent transactions", methodStart, StringComparison.Ordinal);
        var methodContent = content[methodStart..methodEnd];

        methodContent.Should().Contain("NormalizeReportDate");
        methodContent.Should().NotContain("ToUniversalTime()", "ngày do người dùng chọn phải giữ nguyên ngày lịch");
        content.Should().Contain("DateTime.SpecifyKind(value.Date, DateTimeKind.Utc)");
    }
}
