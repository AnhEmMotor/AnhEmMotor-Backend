using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetPnlReportForChat;

public class GetPnlReportForChatQueryHandler(
    IStatisticalAnalyticsRepository analyticsRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetPnlReportForChatQuery, Result<ChatToolEnvelope<ChatPnlReportDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatPnlReportDto>>> Handle(
        GetPnlReportForChatQuery request,
        CancellationToken cancellationToken)
    {
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate, dateProvider);

        // ponytail: IStatisticalAnalyticsRepository.GetPnlReportAsync chỉ nhận (month, year) theo tháng
        // dương lịch, chưa hỗ trợ khoảng ngày tuỳ ý — dùng tháng chứa "Đến ngày" đã resolve; nếu cần P&L
        // theo khoảng ngày chính xác thì phải sửa repo, không bịa thêm ở đây.
        var vnEnd = end.ToOffset(TimeSpan.FromHours(7)).Date;
        var report = await analyticsRepository.GetPnlReportAsync(vnEnd.Month, vnEnd.Year, cancellationToken)
            .ConfigureAwait(false);

        var dto = new ChatPnlReportDto
        {
            Period = report.Period,
            Revenue = report.TotalRevenue,
            CostOfGoods = report.TotalCostOfGoodsSold,
            GrossProfit = report.GrossProfit,
            Expenses = report.TotalOperatingExpenses,
            NetProfit = report.NetProfit,
        };

        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IStatisticalAnalyticsRepository.GetPnlReportAsync",
            new Dictionary<string, string> { ["Khoảng thời gian"] = ChatToolDateRange.FormatVietnamRange(start, end) },
            "bao-cao-lai-lo",
            "VND",
            [$"Dữ liệu tính theo tháng dương lịch {vnEnd.Month}/{vnEnd.Year} (chứa \"Đến ngày\"), chưa hỗ trợ khoảng ngày tuỳ ý."]);

        return ChatToolEnvelope<ChatPnlReportDto>.WrapSingle(dto, meta);
    }
}
