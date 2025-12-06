using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

public class AsyncQueryableExecuter : IAsyncQueryableExecuter
{
    public Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
    { return query.ToListAsync(cancellationToken); }

    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
    { return query.FirstOrDefaultAsync(cancellationToken); }

    public Task<T?> FirstOrDefaultAsync<T>(
        IQueryable<T> query,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    { return query.FirstOrDefaultAsync(predicate, cancellationToken); }

    public Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
    { return query.CountAsync(cancellationToken); }

    public Task<bool> AnyAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
    { return query.AnyAsync(cancellationToken); }

    public IQueryable<T> Include<T>(IQueryable<T> query, string navigationPropertyPath) where T : class
    { return query.Include(navigationPropertyPath); }
}
