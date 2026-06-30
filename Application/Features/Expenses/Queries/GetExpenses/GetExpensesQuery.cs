using Application.Common.Models;
using MediatR;

namespace Application.Features.Expenses.Queries.GetExpenses;

public sealed record GetExpensesQuery : IRequest<Result<List<ExpenseResponse>>>;
