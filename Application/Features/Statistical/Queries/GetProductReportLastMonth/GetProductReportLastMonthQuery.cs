using Application.ApiContracts.Statistical.Responses;
using MediatR;

namespace Application.Features.Statistical.Queries.GetProductReportLastMonth;

public sealed record GetProductReportLastMonthQuery : IRequest<IEnumerable<ProductReportResponse>>;
