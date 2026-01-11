using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Statistical.Queries.GetProductReportLastMonth;

public sealed record GetProductReportLastMonthQuery : IRequest<Result<IEnumerable<ProductReportResponse>>>;
