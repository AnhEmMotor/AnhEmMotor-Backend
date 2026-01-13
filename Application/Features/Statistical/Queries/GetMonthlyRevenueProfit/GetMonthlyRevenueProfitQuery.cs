using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Statistical.Queries.GetMonthlyRevenueProfit;

public sealed record GetMonthlyRevenueProfitQuery : IRequest<Result<IEnumerable<MonthlyRevenueProfitResponse>>>
{
    public int Months { get; set; } = 12;
}
