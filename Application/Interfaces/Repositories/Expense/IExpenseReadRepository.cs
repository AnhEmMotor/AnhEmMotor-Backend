using ExpenseEntity = Domain.Entities.Expense;

namespace Application.Interfaces.Repositories.Expense;

public interface IExpenseReadRepository
{
    public Task<List<ExpenseEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    public Task<ExpenseEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

