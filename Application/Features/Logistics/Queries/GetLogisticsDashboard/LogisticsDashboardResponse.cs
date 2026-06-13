using Application.ApiContracts.Logistics.Responses;
using System;

namespace Application.Features.Logistics.Queries.GetLogisticsDashboard;

public class LogisticsDashboardResponse

{
    public LogisticsDashboardSummaryResponse Summary { get; set; } = new();

    public Dictionary<string, int> FulfillmentFunnel { get; set; } = [];

    public List<LogisticsTrendPointResponse> Trends { get; set; } = [];

    public List<CarrierScoreRowResponse> CarrierScorecard { get; set; } = [];

    public List<LogisticsExceptionRowResponse> Exceptions { get; set; } = [];
}
