using Application.Common.Models;
using MediatR;

namespace Application.Features.SalesReports.Queries.GetSalesReport;

public sealed record GetSalesReportQuery : IRequest<Result<SalesReportResponse>>;
