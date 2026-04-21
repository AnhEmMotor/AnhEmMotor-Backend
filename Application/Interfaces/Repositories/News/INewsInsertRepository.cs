using Domain.Entities;

namespace Application.Interfaces.Repositories.News
{
    public interface INewsInsertRepository
    {
        void Add(Domain.Entities.News news);
    }
}
