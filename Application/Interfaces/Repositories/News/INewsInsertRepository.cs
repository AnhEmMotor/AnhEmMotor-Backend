
namespace Application.Interfaces.Repositories.News
{
    public interface INewsInsertRepository
    {
        public void Add(Domain.Entities.News news);
    }
}
