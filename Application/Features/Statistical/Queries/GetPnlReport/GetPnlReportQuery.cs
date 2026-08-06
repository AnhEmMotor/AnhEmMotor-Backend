using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Statistical.Queries.GetPnlReport;

public sealed record GetPnlReportQuery(int Month, int Year) : IRequest<Result<PnlReportResponse>>;
