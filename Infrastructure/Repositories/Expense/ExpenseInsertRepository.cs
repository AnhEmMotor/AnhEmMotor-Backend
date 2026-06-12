using ExpenseEntity = AnhEmMotor.Domain.Entities.Expense;
using Application.Interfaces.Repositories.Expense;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Expense;

public class ExpenseInsertRepository(ApplicationDBContext context) : IExpenseInsertRepository
{
    public void Add(ExpenseEntity expense) => context.Set<ExpenseEntity>().Add(expense);
}
