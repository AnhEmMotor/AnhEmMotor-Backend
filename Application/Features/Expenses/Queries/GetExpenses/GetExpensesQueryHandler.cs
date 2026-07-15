using Application.Common.Models;
using Application.Features.Expenses.Responses;
using Application.Interfaces.Repositories.Expense;
using Domain.Entities;
using Domain.Enums;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Expenses.Queries.GetExpenses;

public class GetExpensesQueryHandler(IExpenseReadRepository expenseReadRepository) : IRequestHandler<GetExpensesQuery, Result<PagedResult<ExpenseResponse>>>
{
	public async Task<Result<PagedResult<ExpenseResponse>>> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
	{
		var result = await expenseReadRepository
			.GetPagedAsync<ExpenseResponse>(request.SieveModel, cancellationToken)
			.ConfigureAwait(false);
		return Result<PagedResult<ExpenseResponse>>.Success(result);
	}
}
