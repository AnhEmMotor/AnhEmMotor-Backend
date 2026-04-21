using Application.Interfaces.Repositories.News;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.News
{
    public class NewsInsertRepository(ApplicationDBContext context) : INewsInsertRepository
    {
        public void Add(Domain.Entities.News news)
        {
            context.News.Add(news);
        }
    }
}
