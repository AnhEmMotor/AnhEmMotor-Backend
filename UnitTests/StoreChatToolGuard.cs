using FluentAssertions;
using System.Text.Json;

namespace UnitTests;

public class StoreChatToolGuard
{
    private static readonly string[] ApprovedToolNames =["search_products", "get_product_detail", "get_product_stock", "get_product_price_list", "list_brands", "escalate_to_staff", ];

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "AnhEmMotor-Backend.sln")))
            dir = dir.Parent;
        dir.Should().NotBeNull("phải tìm được thư mục gốc chứa file .sln");
        return dir!.FullName;
    }

    private static JsonElement[] LoadCatalog()
    {
        var path = Path.Combine(RepoRoot(), "SharedConfig", "chat-tools-catalog.store.json");
        var content = File.ReadAllText(path);
        return JsonSerializer.Deserialize<JsonElement[]>(content)!;
    }

    [Fact(DisplayName = "GUARD_STORE_01 - Catalog tool store khớp chính xác danh sách đã duyệt Stage 02")]
    public void Catalog_ChiChua_ToolDaDuyet()
    {
        var names = LoadCatalog().Select(e => e.GetProperty("name").GetString()).ToList();
        names.Should()
            .BeEquivalentTo(
                ApprovedToolNames,
                "thêm/xoá tool ở catalog store mà quên cập nhật test này là tín hiệu cần review lại, " +
                    "không phải lỗi test");
    }

    [Fact(DisplayName = "GUARD_STORE_02 - Chỉ đúng 1 tool ghi (escalate_to_staff), còn lại là tool đọc")]
    public void Catalog_ChiCoDungMotToolGhi()
    {
        var catalog = LoadCatalog();
        var writeTools = catalog
            .Where(e => e.TryGetProperty("is_write", out var w) && w.GetBoolean())
            .Select(e => e.GetProperty("name").GetString())
            .ToList();
        writeTools.Should()
            .BeEquivalentTo(
                ["escalate_to_staff"],
                "persona store chỉ được phép ghi qua đúng 1 tool chuyển nhân viên, không tool nào khác");
    }
}
