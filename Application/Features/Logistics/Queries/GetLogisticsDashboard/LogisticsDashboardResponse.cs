using Application.ApiContracts.Logistics.Responses;
using System;

namespace Application.Features.Logistics.Queries.GetLogisticsDashboard;

public class LogisticsDashboardResponse

{
    public LogisticsDashboardSummaryResponse Summary { get; set; } = new();
}
