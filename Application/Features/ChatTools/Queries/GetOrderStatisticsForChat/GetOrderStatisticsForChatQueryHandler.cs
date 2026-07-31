using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetOrderStatisticsForChat;

public class GetOrderStatisticsForChatQueryHandler(
    IStatisticalReadRepository statisticalReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetOrderStatisticsForChatQuery, Result<ChatToolEnvelope<ChatOrderStatisticsDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatOrderStatisticsDto>>> Handle(
        GetOrderStatisticsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate, dateProvider);
        var statusCounts = await statisticalReadRepository.GetOrderStatusCountsAsync(cancellationToken)
            .ConfigureAwait(false);
        var countByStatus = statusCounts
            .Where(s => s.StatusName is not null)
            .ToDictionary(s => s.StatusName!, s => s.OrderCount);
        var dto = new ChatOrderStatisticsDto
        {
            TotalOrders = countByStatus.Values.Sum(),
            CountByStatus = countByStatus
        };
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IStatisticalReadRepository.GetOrderStatusCountsAsync",
            new Dictionary<string, string> { ["Khoảng thời gian"] = ChatToolDateRange.FormatVietnamRange(start, end) },
            "thong-ke-don-hang",
            null,
            [
                "Repository GetOrderStatusCountsAsync hiện không hỗ trợ lọc theo ngày — số liệu là tổng toàn bộ đơn hàng, khoảng thời gian ở trên chỉ mang tính tham chiếu."
            ]);
        return ChatToolEnvelope<ChatOrderStatisticsDto>.WrapSingle(dto, meta);
    }
}
