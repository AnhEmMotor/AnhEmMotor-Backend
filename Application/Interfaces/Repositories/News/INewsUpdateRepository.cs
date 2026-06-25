using Domain.Entities;

namespace Application.Interfaces.Repositories.News
{
    public interface INewsUpdateRepository
    {
        public void Update(Domain.Entities.News news);

        public void RemoveLinkedProducts(IEnumerable<NewsProduct> linkedProducts);
    }
}
