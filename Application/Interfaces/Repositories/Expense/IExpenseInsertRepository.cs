using ExpenseEntity = Domain.Entities.Expense;

namespace Application.Interfaces.Repositories.Expense;

public interface IExpenseInsertRepository
{
    public void Add(ExpenseEntity expense);
}

