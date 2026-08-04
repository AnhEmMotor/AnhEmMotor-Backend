using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.LogisticsDashboard;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetLogisticsDashboardForChat;

public class GetLogisticsDashboardForChatQueryHandler(
    ILogisticsDashboardRepository logisticsDashboardRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetLogisticsDashboardForChatQuery, Result<ChatToolEnvelope<ChatLogisticsDashboardDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatLogisticsDashboardDto>>> Handle(
        GetLogisticsDashboardForChatQuery request,
        CancellationToken cancellationToken)
    {
        var now = dateProvider.UtcNow.UtcDateTime;
        DateTime from = request.Range switch
        {
            "month" => now.AddDays(-30),
            "year" => now.AddDays(-365),
            _ => now.AddDays(-1)
        };
        var dashboard = await logisticsDashboardRepository.GetDashboardAsync(from, cancellationToken)
            .ConfigureAwait(false);
        var dto = new ChatLogisticsDashboardDto
        {
            FulfillmentWorkload = dashboard.Summary.FulfillmentWorkload,
            FulfillmentWorkloadIsOverload = dashboard.Summary.FulfillmentWorkloadIsOverload,
            PendingUnreconciledCod = dashboard.Summary.PendingUnreconciledCod,
            OtifRate = dashboard.Summary.OtifRate,
            ReturnsClaimsRate = dashboard.Summary.ReturnsClaimsRate,
            ExceptionCount = dashboard.Exceptions.Count
        };
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "ILogisticsDashboardRepository.GetDashboardAsync",
            new Dictionary<string, string> { ["Khoảng thời gian"] = request.Range },
            "tong-quan-logistics",
            null);
        return ChatToolEnvelope<ChatLogisticsDashboardDto>.WrapSingle(dto, meta);
    }
}
