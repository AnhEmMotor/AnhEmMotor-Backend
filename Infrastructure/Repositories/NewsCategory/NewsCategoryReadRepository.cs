using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.NewsCategory;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.NewsCategory
{
    public class NewsCategoryReadRepository(ApplicationDBContext context, ISievePaginator paginator) : INewsCategoryReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<Domain.Entities.NewsCategory, bool>>? filter = null,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode);
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return paginator.ApplyAsync<Domain.Entities.NewsCategory, TResponse>(query, sieveModel, mode, cancellationToken);
        }

        internal IQueryable<Domain.Entities.NewsCategory> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = context.NewsCategories.AsQueryable();
            if (mode == DataFetchMode.ActiveOnly)
            {
                query = query.Where(n => n.IsActive);
            }
            return query;
        }

        public Task<Domain.Entities.NewsCategory?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return context.NewsCategories
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        }

        public Task<Domain.Entities.NewsCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            return context.NewsCategories
                .FirstOrDefaultAsync(n => string.Compare(n.Slug, slug) == 0, cancellationToken);
        }
    }
}
