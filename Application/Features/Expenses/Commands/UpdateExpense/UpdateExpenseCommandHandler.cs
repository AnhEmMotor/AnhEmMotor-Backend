using Application.Common.Models;
using Application.Features.Expenses.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Expense;
using Domain.Enums;
using MediatR;

namespace Application.Features.Expenses.Commands.UpdateExpense;

public class UpdateExpenseCommandHandler(
    IExpenseReadRepository readRepository,
    IExpenseUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateExpenseCommand, Result<ExpenseResponse>>
{
    public async Task<Result<ExpenseResponse>> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (expense == null)
            return Result<ExpenseResponse>.Failure("Không tìm thấy khoản chi phí.");
        expense.Name = request.Name;
        expense.Amount = request.Amount;
        expense.ExpenseDate = request.ExpenseDate;
        expense.Category = (ExpenseCategory)request.Category;
        expense.Note = request.Note;
        expense.UpdatedAt = DateTime.UtcNow;
        updateRepository.Update(expense);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<ExpenseResponse>.Success(
            new ExpenseResponse
            {
                Id = expense.Id,
                Name = expense.Name,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Category = (int)expense.Category,
                CategoryText = expense.Category == ExpenseCategory.Fixed ? "Cố định" : "Biến đổi",
                Note = expense.Note,
                CreatedAt = expense.CreatedAt
            });
    }
}
