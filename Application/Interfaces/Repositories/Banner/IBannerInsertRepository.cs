using Domain.Entities;

namespace Application.Interfaces.Repositories.Banner
{
    public interface IBannerInsertRepository
    {
        void Add(Domain.Entities.Banner banner);
    }
}
