using System.Text.Json;
using Application.Features.ChatTools.Common;
using Application.Features.ChatTools.Queries.GetOrderStatusForChat;
using Application.Features.ChatTools.Queries.GetSalesSummaryForChat;
using FluentAssertions;

namespace UnitTests.Snapshots;

/// <summary>
/// Stage 16.7 — chống DTO trôi. Không thêm package snapshot mới (Verify.Xunit) chỉ cho việc này;
/// so trực tiếp JSON serialize với file .json commit sẵn bằng FluentAssertions đã có sẵn trong dự án.
/// Field bị đổi tên/xoá mà quên cập nhật tool → test đỏ ngay.
/// </summary>
public class ChatToolEnvelopeContract
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private static readonly DateTimeOffset FixedAsOf = new(2026, 7, 26, 9, 15, 0, TimeSpan.FromHours(7));

    private static string SnapshotDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "AnhEmMotor-Backend.sln")))
            dir = dir.Parent;
        if (dir is null)
            throw new InvalidOperationException("Không tìm được thư mục gốc chứa file .sln");
        return Path.Combine(dir.FullName, "UnitTests", "Snapshots");
    }

    [Fact(DisplayName = "CONTRACT_01 - Envelope doanh thu (list) giữ nguyên hợp đồng JSON")]
    public void ChatToolEnvelope_ListShape_GiuNguyenHopDong()
    {
        var meta = new ChatToolEnvelopeMeta(
            FixedAsOf,
            "IStatisticalReadRepository.GetDailyRevenueAsync",
            new Dictionary<string, string> { ["Loại trừ"] = "Đơn huỷ, đơn nháp, bản ghi soft-delete" },
            "doanh-thu",
            "VND");
        var inner = new ChatToolResult<ChatDailyRevenueDto>(
            [new ChatDailyRevenueDto { ReportDay = new DateOnly(2026, 7, 26), TotalRevenue = 1240000000 }],
            1,
            false);
        var envelope = ChatToolEnvelope<ChatDailyRevenueDto>.Wrap(inner, meta);

        AssertMatchesSnapshot(envelope, "chat-tool-envelope-list.json");
    }

    [Fact(DisplayName = "CONTRACT_02 - Envelope đối tượng đơn (WrapSingle) giữ nguyên hợp đồng JSON")]
    public void ChatToolEnvelope_SingleShape_GiuNguyenHopDong()
    {
        var meta = new ChatToolEnvelopeMeta(
            FixedAsOf,
            "IOutputReadRepository.GetByIdWithDetailsAsync",
            new Dictionary<string, string>(),
            "so-don-hang",
            "VND");
        var dto = new ChatOrderStatusDto { OrderId = 123, StatusId = "completed", Total = 5000000 };
        var envelope = ChatToolEnvelope<ChatOrderStatusDto>.WrapSingle(dto, meta);

        AssertMatchesSnapshot(envelope, "chat-tool-envelope-single.json");
    }

    private static void AssertMatchesSnapshot<T>(T value, string fileName)
    {
        var actual = JsonSerializer.Serialize(value, Options);
        var path = Path.Combine(SnapshotDir(), fileName);

        if (!File.Exists(path))
        {
            Directory.CreateDirectory(SnapshotDir());
            File.WriteAllText(path, actual);
            throw new InvalidOperationException(
                $"Chưa có snapshot {fileName} — đã tạo mới, hãy review rồi commit file này.");
        }

        var expected = File.ReadAllText(path);
        actual.Should().Be(expected, $"shape của envelope đã đổi so với snapshot commit sẵn {fileName} — nếu đây là" +
            " thay đổi có chủ đích, xoá file snapshot và chạy lại test để tạo bản mới rồi commit.");
    }
}
