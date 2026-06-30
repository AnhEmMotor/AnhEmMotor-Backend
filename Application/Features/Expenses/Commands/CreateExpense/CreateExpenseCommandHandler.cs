using Application.Common.Models;
using Application.Interfaces.Repositories.Expense;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Expenses.Commands.CreateExpense;

public class CreateExpenseCommandHandler(IExpenseInsertRepository expenseInsertRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateExpenseCommand, Result<ExpenseResponse>>
{
    public async Task<Result<ExpenseResponse>> Handle(
        CreateExpenseCommand request,
        CancellationToken cancellationToken)
    {
        var expense = new Expense
        {
            Name = request.Name,
            Amount = request.Amount,
            ExpenseDate = request.ExpenseDate,
            Category = (ExpenseCategory)request.Category,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
        };

        expenseInsertRepository.Add(expense);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = new ExpenseResponse
        {
            Id = expense.Id,
            Name = expense.Name,
            Amount = expense.Amount,
            ExpenseDate = expense.ExpenseDate,
            Category = (int)expense.Category,
            Note = expense.Note,
            CreatedAt = expense.CreatedAt
        };

        return Result<ExpenseResponse>.Success(response);
    }
}
