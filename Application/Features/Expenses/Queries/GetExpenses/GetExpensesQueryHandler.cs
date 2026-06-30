using Application.Common.Models;
using Application.Interfaces.Repositories.Expense;
using MediatR;
using Domain.Entities;
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
                Category = e.Category,
                CategoryText = e.Category == Domain.Enums.ExpenseCategory.Fixed ? "Cố định" : "Biến đổi",
                Note = e.Note,
                CreatedAt = e.CreatedAt
            })
            .ToList();

        return Result<List<ExpenseResponse>>.Success(response);
    }
}

public class ExpenseResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public Domain.Enums.ExpenseCategory Category { get; set; }
    public string CategoryText { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
