using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Statistical.Queries.GetAdminWarehouseReport;

public sealed record GetAdminWarehouseReportQuery : IRequest<Result<AdminWarehouseReportResponse>>
{
    public DateTime? StartDate { get; init; }

    public DateTime? EndDate { get; init; }
}
