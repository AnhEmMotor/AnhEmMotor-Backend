namespace Application.Interfaces.Repositories;

public interface IAsyncQueryableExecuter
{
    Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync<T>(
        IQueryable<T> query,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default);

    IQueryable<T> Include<T>(IQueryable<T> query, string navigationPropertyPath) where T : class;
}
