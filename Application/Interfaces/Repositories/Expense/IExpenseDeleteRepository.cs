using ExpenseEntity = AnhEmMotor.Domain.Entities.Expense;

namespace Application.Interfaces.Repositories.Expense;

public interface IExpenseDeleteRepository
{
    void Remove(ExpenseEntity expense);
}
