using Domain.Entities;
using Domain.Constants;

namespace Application.Interfaces.Repositories.News
{
    public interface INewsReadRepository
    {
        IQueryable<Domain.Entities.News> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);
        Task<Domain.Entities.News?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<Domain.Entities.News?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    }
}

