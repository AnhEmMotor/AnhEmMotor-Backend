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

        public Task<Domain.Entities.News?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return context.News.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        }

        public Task<Domain.Entities.News?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            return context.News.FirstOrDefaultAsync(n => string.Compare(n.Slug, slug) == 0, cancellationToken);
        }
    }
}

