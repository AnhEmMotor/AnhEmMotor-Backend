using ExpenseEntity = AnhEmMotor.Domain.Entities.Expense;

namespace Application.Interfaces.Repositories.Expense;

public interface IExpenseUpdateRepository
{
    public void Update(ExpenseEntity expense);
}

