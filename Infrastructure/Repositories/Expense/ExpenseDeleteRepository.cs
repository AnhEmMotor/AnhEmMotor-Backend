using Application.Interfaces.Repositories.Expense;
using Infrastructure.DBContexts;
using ExpenseEntity = Domain.Entities.Expense;

namespace Infrastructure.Repositories.Expense;

public class ExpenseDeleteRepository(ApplicationDBContext context) : IExpenseDeleteRepository
{
    public void Remove(ExpenseEntity expense) => context.Set<ExpenseEntity>().Remove(expense);
}
