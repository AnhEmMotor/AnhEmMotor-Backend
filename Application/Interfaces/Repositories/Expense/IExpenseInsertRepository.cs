using ExpenseEntity = AnhEmMotor.Domain.Entities.Expense;

namespace Application.Interfaces.Repositories.Expense;

public interface IExpenseInsertRepository
{
    void Add(ExpenseEntity expense);
}
