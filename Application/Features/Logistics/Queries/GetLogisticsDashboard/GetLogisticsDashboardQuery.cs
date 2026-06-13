using MediatR;

namespace Application.Features.Logistics.Queries.GetLogisticsDashboard;

public class GetLogisticsDashboardQuery : IRequest<LogisticsDashboardResponse>
{
    public string Range { get; init; } = "today";
}

