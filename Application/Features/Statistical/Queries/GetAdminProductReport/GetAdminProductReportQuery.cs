using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Statistical.Queries.GetAdminProductReport;

public sealed record GetAdminProductReportQuery : IRequest<Result<AdminProductReportResponse>>;
