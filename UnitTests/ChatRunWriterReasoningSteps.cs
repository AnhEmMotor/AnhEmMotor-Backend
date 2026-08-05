using Domain.Constants;
using FluentAssertions;
using Infrastructure.Services.Ai.Runs;

namespace UnitTests;

public class ChatRunWriterReasoningSteps
{
    private static readonly Dictionary<string, string> Labels = new()
    {
        ["get_sales_summary"] = "Tra doanh thu",
    };

    [Fact(DisplayName = "REASONING_01 - Gộp đúng thinking + tool_start/tool_end theo thứ tự")]
    public void BuildReasoningSteps_GopDungThinkingVaToolTheoThuTu()
    {
        var events = new List<(string Type, string Payload)>
        {
            (ChatRunEventType.Thinking, """{"text":"Tôi sẽ tra doanh thu."}"""),
            (ChatRunEventType.ToolStart, """{"name":"get_sales_summary","summary":"Đang tra doanh thu"}"""),
            (ChatRunEventType.ToolEnd, """{"name":"get_sales_summary","durationMs":120,"summary":"Doanh thu 10 triệu","truncated":false}"""),
        };

        var steps = ChatRunWriter.BuildReasoningSteps(events, Labels);

        steps.Should().HaveCount(2);
        steps[0].Kind.Should().Be("thinking");
        steps[0].Text.Should().Be("Tôi sẽ tra doanh thu.");
        steps[1].Kind.Should().Be("tool");
        steps[1].Name.Should().Be("get_sales_summary");
        steps[1].Label.Should().Be("Tra doanh thu");
        steps[1].Status.Should().Be("done");
        steps[1].Summary.Should().Be("Doanh thu 10 triệu");
        steps[1].DurationMs.Should().Be(120);
    }

    [Fact(DisplayName = "REASONING_02 - tool_end không khớp tool_start nào thì bị bỏ qua, không throw")]
    public void BuildReasoningSteps_ToolEndKhongKhopThiBoQua()
    {
        var events = new List<(string Type, string Payload)>
        {
            (ChatRunEventType.ToolEnd, """{"name":"khong_ton_tai","durationMs":50}"""),
        };

        var steps = ChatRunWriter.BuildReasoningSteps(events, Labels);

        steps.Should().BeEmpty();
    }

    [Fact(DisplayName = "REASONING_03 - Không có event nào thì trả về danh sách rỗng")]
    public void BuildReasoningSteps_KhongCoEventThiRong()
    {
        var steps = ChatRunWriter.BuildReasoningSteps([], Labels);

        steps.Should().BeEmpty();
    }

    [Fact(DisplayName = "REASONING_04 - Tên tool không có trong catalog thì dùng chính tên đó làm label")]
    public void BuildReasoningSteps_ToolKhongCoTrongCatalogThiDungTenLamLabel()
    {
        var events = new List<(string Type, string Payload)>
        {
            (ChatRunEventType.ToolStart, """{"name":"tool_moi_chua_co_label"}"""),
        };

        var steps = ChatRunWriter.BuildReasoningSteps(events, Labels);

        steps.Should().ContainSingle();
        steps[0].Label.Should().Be("tool_moi_chua_co_label");
        steps[0].Status.Should().Be("running");
    }
}
