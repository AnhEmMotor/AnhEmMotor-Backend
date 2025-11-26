using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetProductReportLastMonth;

public sealed record GetProductReportLastMonthQuery : IRequest<IEnumerable<ProductReportDto>>;
