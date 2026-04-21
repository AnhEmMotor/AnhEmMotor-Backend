using Application.Interfaces.Repositories.Banner;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Banner
{
    public class BannerInsertRepository(ApplicationDBContext context) : IBannerInsertRepository
    {
        public void Add(Domain.Entities.Banner banner)
        {
            context.Banners.Add(banner);
        }
    }
}
