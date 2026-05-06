using Application.Interfaces.Repositories.News;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.News
{
    public class NewsUpdateRepository(ApplicationDBContext context) : INewsUpdateRepository
    {
        public void Update(Domain.Entities.News news)
        {
            context.News.Update(news);
        }
    }
}
