using Application.Api.Contracts.Statistical.Responses;
using MediatR;
using System;

namespace Application.Features.Statistical.Queries.GetWorkshopDashboard;

public class GetWorkshopDashboardQuery : IRequest<WorkshopDashboardResponse>
{
    public DateTimeOffset FromDate { get; set; }
    public DateTimeOffset ToDate { get; set; }

    public GetWorkshopDashboardQuery(DateTimeOffset fromDate, DateTimeOffset toDate)
    {
        FromDate = fromDate;
        ToDate = toDate;
    }
}
