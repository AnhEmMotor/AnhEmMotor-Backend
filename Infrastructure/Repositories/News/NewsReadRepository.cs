using Application.Interfaces.Repositories.News;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.News
{
    public class NewsReadRepository(ApplicationDBContext context) : INewsReadRepository
    {
        public IQueryable<Domain.Entities.News> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = context.News.AsQueryable();
            if (mode == DataFetchMode.ActiveOnly)
            {
                query = query.Where(n => n.IsPublished);
            }
            return query;
        }

        public async Task<Domain.Entities.News?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await context.News.FirstOrDefaultAsync(n => n.Id == id, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Domain.Entities.News?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            return await context.News.FirstOrDefaultAsync(n => string.Compare(n.Slug, slug) == 0, cancellationToken).ConfigureAwait(false);
        }
    }
}

