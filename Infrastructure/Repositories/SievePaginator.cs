using Application.Interfaces.Repositories;
using Application.Sieve;
using Domain.Enums;
using Domain.Shared;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace Infrastructure.Repositories
{
    public class SievePaginator(ISieveProcessor sieveProcessor) : ICustomSievePaginator
    {
        public async Task<PagedResult<TResponse>> ApplyAsync<TEntity, TResponse>(
            IQueryable<TEntity> query,
            SieveModel sieveModel,
            DataFetchMode? defaultSortMode,
            CancellationToken cancellationToken)
            where TEntity : class
        {
            if (defaultSortMode.HasValue)
            {
                SieveHelper.ApplyDefaultSorting(sieveModel, defaultSortMode.Value);
            }

            var totalCount = await sieveProcessor
                .Apply(sieveModel, query, applyPagination: false)
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);

            var pagedQuery = sieveProcessor.Apply(sieveModel, query);

            var entities = await pagedQuery
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var responses = entities.Adapt<List<TResponse>>();

            return new PagedResult<TResponse>(
                responses,
                totalCount,
                sieveModel.Page ?? 1,
                sieveModel.PageSize ?? 10);
        }
    }
}
