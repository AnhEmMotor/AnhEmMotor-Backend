using System.Text.RegularExpressions;
using FluentAssertions;

namespace UnitTests;

/// <summary>
/// Guard chống tạo migration vào sai thư mục. EF Core mặc định ghi vào Infrastructure\Migrations\ khi
/// chạy "dotnet ef migrations add" thiếu --output-dir (hoặc AI tự bịa đường dẫn như Migrations\SqlServer),
/// trong khi quy ước dự án bắt buộc: SqlServerMigrations | MySqlMigrations | PostgreSqlMigrations.
/// Khi fail, test tự đọc attribute [DbContext(typeof(...))] để GỢI Ý CÁCH KHẮC PHỤC cụ thể cho từng file.
/// </summary>
public class MigrationLocations
{
    private static readonly string[] AllowedMigrationDirs =
    [
        "SqlServerMigrations",
        "MySqlMigrations",
        "PostgreSqlMigrations"
    ];

    private static readonly Dictionary<string, (string Dir, string Namespace)>
        ContextToTargetDirAndNamespace = new(StringComparer.OrdinalIgnoreCase)
        {
            ["SqlServerDBContext"] = ("SqlServerMigrations", "Infrastructure.SqlServerMigrations"),
            ["MySqlDbContext"] = ("MySqlMigrations", "Infrastructure.MySqlMigrations"),
            ["PostgreSqlDbContext"] = ("PostgreSqlMigrations", "Infrastructure.PostgreSqlMigrations")
        };

    [Fact(
        DisplayName = "MIGRATION_01 - Guard - không có file migration nào nằm ngoài 3 thư mục provider chuẩn")]
    public void MigrationFiles_PhaiNamTrongCacThuMucChuan()
    {
        var infraRoot = FindInfraRoot();
        var offenders = Directory
            .EnumerateFiles(infraRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                           !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Where(IsMigrationRelatedFile)
            .Where(path =>
            {
                var containingDir = new DirectoryInfo(Path.GetDirectoryName(path)!).Name;
                return !AllowedMigrationDirs.Contains(containingDir, StringComparer.OrdinalIgnoreCase);
            })
            .OrderBy(path => path)
            .ToList();

        if (offenders.Count == 0)
        {
            return;
        }

        var details = string.Join(
            Environment.NewLine,
            offenders.Select(path => $"   - {Path.GetRelativePath(infraRoot, path)}{Environment.NewLine}" +
                                     DescribeFix(infraRoot, path)));

        Assert.Fail(
            $"Phát hiện {offenders.Count} file migration đang ở SAI thư mục:{Environment.NewLine}{details}{Environment.NewLine}" +
            Environment.NewLine +
            "Gợi ý cách khắc phục:" + Environment.NewLine +
            "  1. Dịch chuyển file migration đã tạo về đúng vị trí (cả file .cs và .Designer.cs nếu có)" +
            " theo thư mục đích được gợi ý ở trên." + Environment.NewLine +
            "  2. Sửa namespace của các file vừa dịch chuyển thành namespace của thư mục đích" +
            " (mỗi provider có một namespace riêng, xem cột 'namespace đúng' ở trên)." + Environment.NewLine +
            "  3. Kiểm tra migration này đã có đủ cho cả 3 provider chưa (SQL Server/MySQL/PostgreSQL);" +
            " nếu thiếu, tạo bản còn thiếu bằng .\\add-migration.ps1 hoặc lệnh --output-dir tương ứng" +
            " (nhớ tuân thủ quy tắc 1 nhánh = 1 migration)." + Environment.NewLine +
            "  4. Xoá thư mục lạ còn trống sau khi dịch chuyển.");
    }

    [Fact(
        DisplayName = "MIGRATION_02 - Guard - mỗi provider phải có đúng 1 ModelSnapshot tại thư mục chuẩn")]
    public void MoiProvider_CoMotModelSnapshot()
    {
        var infraRoot = FindInfraRoot();
        foreach (var dir in AllowedMigrationDirs)
        {
            var snapshots = Directory.EnumerateFiles(
                    Path.Combine(infraRoot, dir), "*ModelSnapshot.cs", SearchOption.TopDirectoryOnly)
                .ToList();
            snapshots.Should().HaveCount(1, $"{dir} phải chứa đúng 1 ModelSnapshot của provider đó");
        }
    }

    /// <summary>File được coi là migration-related khi nằm dưới thư mục tên chứa "Migration"
    /// hoặc là ModelSnapshot — đủ rộng để bắt cả Migrations\SqlServer kiểu lạc đề.</summary>
    private static bool IsMigrationRelatedFile(string path)
    {
        var segments = Path.GetDirectoryName(path)!
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return segments.Any(segment =>
                   segment.Contains("Migration", StringComparison.OrdinalIgnoreCase)) ||
               Path.GetFileName(path).EndsWith("ModelSnapshot.cs", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Đọc attribute [DbContext(typeof(Xxx))] trong file (hoặc file .Designer.cs cùng tên) để suy ra
    /// thư mục đích + namespace đúng, phục vụ gợi ý khắc phục ngay trong thông báo fail.
    /// </summary>
    private static string DescribeFix(string infraRoot, string path)
    {
        var content = ReadContentWithDesignerFallback(path, out var designerNote);
        var contextMatch = Regex.Match(content, @"DbContext\(typeof\((?<context>\w+)\)\)");

        if (!contextMatch.Success)
        {
            return $"     → không xác định được [DbContext(typeof(...))] trong file này{designerNote}" +
                   " — hãy dời về thư mục provider khớp với context của migration.";
        }

        var contextName = contextMatch.Groups["context"].Value;
        if (!ContextToTargetDirAndNamespace.TryGetValue(contextName, out var target))
        {
            return $"     → context '{contextName}' không thuộc 3 provider chuẩn{designerNote}.";
        }

        return $"     → context: {contextName} | cần chuyển vào: Infrastructure/{target.Dir}/" +
               $" | namespace đúng: {target.Namespace}{designerNote}";
    }

    private static string ReadContentWithDesignerFallback(string path, out string designerNote)
    {
        var content = File.ReadAllText(path);
        designerNote = string.Empty;
        if (content.Contains("DbContext(typeof("))
        {
            return content;
        }

        var designer = Path.Combine(
            Path.GetDirectoryName(path)!,
            Path.GetFileNameWithoutExtension(path) + ".Designer.cs");
        if (File.Exists(designer))
        {
            var designerContent = File.ReadAllText(designer);
            if (designerContent.Contains("DbContext(typeof("))
            {
                designerNote = " (tra từ file .Designer.cs)";
                return designerContent;
            }
        }

        return content;
    }

    private static string FindInfraRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null &&
               !Directory.Exists(Path.Combine(
                   dir.FullName,
                   "Infrastructure",
                   "SqlServerMigrations")))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy thư mục Infrastructure (chứa SqlServerMigrations) từ " +
                AppContext.BaseDirectory);
        }

        return Path.Combine(dir.FullName, "Infrastructure");
    }
}
