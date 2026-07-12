using Application.Features.Logistics.Queries.GetLogisticsDashboard;
using System;

namespace Application.Interfaces.Repositories.LogisticsDashboard;

public interface ILogisticsDashboardRepository
{
    public Task<LogisticsDashboardResponse> GetDashboardAsync(DateTime fromDate, CancellationToken cancellationToken);
}
