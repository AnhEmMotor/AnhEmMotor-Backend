using Application.Interfaces.Repositories.News;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.News
{
    public class NewsDeleteRepository(ApplicationDBContext context) : INewsDeleteRepository
    {
        public void Delete(Domain.Entities.News news)
        {
            context.News.Remove(news);
        }
    }
}
