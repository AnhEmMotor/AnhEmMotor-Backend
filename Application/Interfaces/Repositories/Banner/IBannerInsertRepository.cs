using Domain.Entities;

namespace Application.Interfaces.Repositories.Banner
{
    public interface IBannerInsertRepository
    {
        public void Add(Domain.Entities.Banner banner);
    }
}
