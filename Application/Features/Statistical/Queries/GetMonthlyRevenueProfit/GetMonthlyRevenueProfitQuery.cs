using Application.ApiContracts.Staticals;
using MediatR;

namespace Application.Features.Statistical.Queries.GetMonthlyRevenueProfit;

public sealed record GetMonthlyRevenueProfitQuery(int Months = 12) : IRequest<IEnumerable<MonthlyRevenueProfitResponse>>;
