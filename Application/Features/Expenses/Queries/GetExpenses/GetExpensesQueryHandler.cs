using Application.Common.Models;
using Application.Features.Expenses.Responses;
using Application.Interfaces.Repositories.Expense;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System.Linq;

namespace Application.Features.Expenses.Queries.GetExpenses;

public class GetExpensesQueryHandler(IExpenseReadRepository expenseReadRepository)
: IRequestHandler<GetExpensesQuery, Result<List<ExpenseResponse>>>
{
public async Task<Result<List<ExpenseResponse>>> Handle(
GetExpensesQuery request,
CancellationToken cancellationToken)
{
var expenses = await expenseReadRepository
.GetAllAsync(cancellationToken)
.ConfigureAwait(false);

var response = expenses
.OrderByDescending(e => e.ExpenseDate)
.Select(e => new ExpenseResponse
{
Id = e.Id,
Name = e.Name,
Amount = e.Amount,
ExpenseDate = e.ExpenseDate,
Category = (int)e.Category,
CategoryText = e.Category == ExpenseCategory.Fixed ? "Cố định" : "Biến đổi",
Note = e.Note,
CreatedAt = e.CreatedAt
})
.ToList();

return Result<List<ExpenseResponse>>.Success(response);
}
}
