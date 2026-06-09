using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.News;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.News
{
    public class NewsReadRepository(ApplicationDBContext context, ISievePaginator paginator) : INewsReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<Domain.Entities.News, bool>>? filter = null,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode);
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return paginator.ApplyAsync<Domain.Entities.News, TResponse>(query, sieveModel, mode, cancellationToken);
        }

        internal IQueryable<Domain.Entities.News> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = context.News.Include(n => n.Category).AsQueryable();
            if (mode == DataFetchMode.ActiveOnly)
            {
                query = query.Where(n => n.IsPublished);
            }
            return query;
        }

        public Task<Domain.Entities.News?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return context.News
                .Include(n => n.LinkedProducts)
                    .ThenInclude(lp => lp.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(n => n.LinkedProducts)
                    .ThenInclude(lp => lp.ProductVariantColor)
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        }

        public Task<Domain.Entities.News?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            return context.News
                .Include(n => n.Category)
                .Include(n => n.LinkedProducts)
                    .ThenInclude(lp => lp.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(n => n.LinkedProducts)
                    .ThenInclude(lp => lp.ProductVariantColor)
                .FirstOrDefaultAsync(n => string.Compare(n.Slug, slug) == 0, cancellationToken);
        }
    }
}

