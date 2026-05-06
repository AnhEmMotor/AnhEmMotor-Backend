using Domain.Entities;

namespace Application.Interfaces.Repositories.Banner
{
    public interface IBannerDeleteRepository
    {
        public void Delete(Domain.Entities.Banner banner);
    }
}
