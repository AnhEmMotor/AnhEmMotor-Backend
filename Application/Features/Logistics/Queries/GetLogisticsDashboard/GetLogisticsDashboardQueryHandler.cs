using Application.Interfaces.Repositories.LogisticsDashboard;
using MediatR;
using System;

namespace Application.Features.Logistics.Queries.GetLogisticsDashboard;

public class GetLogisticsDashboardQueryHandler(ILogisticsDashboardRepository logisticsDashboardRepository) : IRequestHandler<GetLogisticsDashboardQuery, LogisticsDashboardResponse>
{
    public async Task<LogisticsDashboardResponse> Handle(
        GetLogisticsDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        DateTime from = request.Range switch
        {
            "month" => now.AddDays(-30),
            "year" => now.AddDays(-365),
            _ => now.AddDays(-1),
        };
        return await logisticsDashboardRepository.GetDashboardAsync(from, cancellationToken);
    }
}
