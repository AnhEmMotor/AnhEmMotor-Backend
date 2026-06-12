using ExpenseEntity = AnhEmMotor.Domain.Entities.Expense;
using Application.Interfaces.Repositories.Expense;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Expense;

public class ExpenseDeleteRepository(ApplicationDBContext context) : IExpenseDeleteRepository
{
    public void Remove(ExpenseEntity expense) => context.Set<ExpenseEntity>().Remove(expense);
}
