namespace Application.Interfaces.Repositories.News
{
    public interface INewsUpdateRepository
    {
        public void Update(Domain.Entities.News news);
        void RemoveLinkedProducts(IEnumerable<Domain.Entities.NewsProduct> linkedProducts);
    }
}
