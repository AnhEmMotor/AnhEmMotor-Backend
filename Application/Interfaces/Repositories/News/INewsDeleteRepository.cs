
namespace Application.Interfaces.Repositories.News
{
    public interface INewsDeleteRepository
    {
        public void Delete(Domain.Entities.News news);
    }
}
