using Application.ApiContracts.Statistical.Responses;
using MediatR;

namespace Application.Features.Statistical.Queries.GetPnlReport;

public sealed record GetPnlReportQuery(int Month, int Year) : IRequest<PnlReportResponse>;
