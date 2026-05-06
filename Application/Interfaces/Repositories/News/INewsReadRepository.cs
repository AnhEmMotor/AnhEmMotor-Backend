using Domain.Constants;

namespace Application.Interfaces.Repositories.News
{
    public interface INewsReadRepository
    {
        public IQueryable<Domain.Entities.News> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<Domain.Entities.News?> GetByIdAsync(int id, CancellationToken cancellationToken);

        public Task<Domain.Entities.News?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    }
}

