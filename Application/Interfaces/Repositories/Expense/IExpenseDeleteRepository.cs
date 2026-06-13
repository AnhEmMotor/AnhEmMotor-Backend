using ExpenseEntity = Domain.Entities.Expense;

namespace Application.Interfaces.Repositories.Expense;

public interface IExpenseDeleteRepository
{
    public void Remove(ExpenseEntity expense);
}

