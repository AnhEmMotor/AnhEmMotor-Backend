using Application.Interfaces.Repositories.Banner;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Banner
{
    public class BannerDeleteRepository(ApplicationDBContext context) : IBannerDeleteRepository
    {
        public void Delete(Domain.Entities.Banner banner)
        {
            context.Banners.Remove(banner);
        }
    }
}
