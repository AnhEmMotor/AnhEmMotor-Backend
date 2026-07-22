using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Expense;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using ExpenseEntity = Domain.Entities.Expense;

namespace Infrastructure.Repositories.Expense;

public class ExpenseReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IExpenseReadRepository
{
    public Task<List<ExpenseEntity>> GetAllAsync(CancellationToken cancellationToken = default) => context.Set<ExpenseEntity>(
        )
        .OrderByDescending(x => x.ExpenseDate)
        .ToListAsync(cancellationToken);

    public Task<ExpenseEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => context.Set<ExpenseEntity>(
        )
        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        CancellationToken cancellationToken = default)
    {
        var query = context.Set<ExpenseEntity>().IgnoreQueryFilters().OrderByDescending(x => x.ExpenseDate);
        return paginator.ApplyAsync<ExpenseEntity, TResponse>(
            query,
            sieveModel,
            defaultSortMode: null,
            cancellationToken);
    }
}
