
namespace Application.Interfaces.Repositories.Banner
{
    public interface IBannerReadRepository
    {
        public Task<Domain.Entities.Banner?> GetByIdAsync(int id, CancellationToken cancellationToken);

        public Task<List<Domain.Entities.Banner>> GetActiveBannersAsync(CancellationToken cancellationToken);
    }
}
