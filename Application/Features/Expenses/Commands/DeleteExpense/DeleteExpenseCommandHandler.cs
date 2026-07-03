using Application.Common.Models;
using Application.Interfaces.Repositories.Expense;
using MediatR;
using Application.Interfaces.Repositories;

namespace Application.Features.Expenses.Commands.DeleteExpense;

public class DeleteExpenseCommandHandler(IExpenseDeleteRepository expenseDeleteRepository, IExpenseReadRepository expenseReadRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteExpenseCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        DeleteExpenseCommand request,
        CancellationToken cancellationToken)
    {
        var expense = await expenseReadRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (expense == null)
            return Result<bool>.Failure("Không tìm thấy khoản chi");

        expenseDeleteRepository.Remove(expense);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<bool>.Success(true);
    }
}
