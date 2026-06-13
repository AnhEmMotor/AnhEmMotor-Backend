using Application.Interfaces.Repositories.Expense;
using Infrastructure.DBContexts;
using ExpenseEntity = Domain.Entities.Expense;

namespace Infrastructure.Repositories.Expense;

public class ExpenseInsertRepository(ApplicationDBContext context) : IExpenseInsertRepository
{
    public void Add(ExpenseEntity expense) => context.Set<ExpenseEntity>().Add(expense);
}
