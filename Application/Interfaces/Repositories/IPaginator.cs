using Domain.Constants;
using Domain.Shared;
using Sieve.Models;
using System;

namespace Application.Interfaces.Repositories
{
    public interface IPaginator
    {
        Task<PagedResult<TResponse>> ApplyAsync<TEntity, TResponse>(
            IQueryable<TEntity> query,
            SieveModel sieveModel,
            DataFetchMode? defaultSortMode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default)
            where TEntity : class;
    }
}
