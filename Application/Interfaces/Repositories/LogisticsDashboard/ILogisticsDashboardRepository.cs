using Application.Features.Logistics.Queries.GetLogisticsDashboard;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.LogisticsDashboard;

public interface ILogisticsDashboardRepository
{
    Task<LogisticsDashboardResponse> GetDashboardAsync(DateTime fromDate, CancellationToken cancellationToken);
}
