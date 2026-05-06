using Application.Interfaces.Repositories.Banner;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Banner
{
    public class BannerUpdateRepository(ApplicationDBContext context) : IBannerUpdateRepository
    {
        public void Update(Domain.Entities.Banner banner)
        {
            context.Banners.Update(banner);
        }
    }
}
