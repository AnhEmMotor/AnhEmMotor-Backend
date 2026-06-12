using ExpenseEntity = AnhEmMotor.Domain.Entities.Expense;
using Application.Interfaces.Repositories.Expense;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Expense;

public class ExpenseReadRepository(ApplicationDBContext context) : IExpenseReadRepository
{
    public Task<List<ExpenseEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => context.Set<ExpenseEntity>().OrderByDescending(x => x.ExpenseDate).ToListAsync(cancellationToken);

    public Task<ExpenseEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => context.Set<ExpenseEntity>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
