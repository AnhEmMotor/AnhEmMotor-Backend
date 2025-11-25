using Domain.Enums;
using Domain.Shared;
using Sieve.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface ICustomSievePaginator
    {
        Task<PagedResult<TResponse>> ApplyAsync<TEntity, TResponse>(
            IQueryable<TEntity> query,
            SieveModel sieveModel,
            DataFetchMode? defaultSortMode = null,
            CancellationToken cancellationToken = default)
        where TEntity : class;
    }
}
