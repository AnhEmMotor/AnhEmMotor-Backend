using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Statistical.Queries.GetMonthlyRevenueProfit;

public sealed record GetMonthlyRevenueProfitQuery(int Months = 12) : IRequest<Result<IEnumerable<MonthlyRevenueProfitResponse>>>;
