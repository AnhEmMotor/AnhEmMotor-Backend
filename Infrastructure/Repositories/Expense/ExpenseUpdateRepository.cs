using ExpenseEntity = AnhEmMotor.Domain.Entities.Expense;
using Application.Interfaces.Repositories.Expense;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Expense;

public class ExpenseUpdateRepository(ApplicationDBContext context) : IExpenseUpdateRepository
{
    public void Update(ExpenseEntity expense) => context.Set<ExpenseEntity>().Update(expense);
}
