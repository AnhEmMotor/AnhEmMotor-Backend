using ExpenseEntity = AnhEmMotor.Domain.Entities.Expense;

namespace Application.Interfaces.Repositories.Expense;

public interface IExpenseReadRepository
{
    Task<List<ExpenseEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ExpenseEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
