using Domain.Entities;

namespace Application.Interfaces.Repositories.Banner
{
    public interface IBannerUpdateRepository
    {
        public void Update(Domain.Entities.Banner banner);
    }
}
