using Application.ApiContracts.Staticals;
using MediatR;

namespace Application.Features.Statistical.Queries.GetProductReportLastMonth;

public sealed record GetProductReportLastMonthQuery : IRequest<IEnumerable<ProductReportResponse>>;
