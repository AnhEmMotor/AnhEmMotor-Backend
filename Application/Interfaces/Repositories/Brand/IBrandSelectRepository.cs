using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandSelectRepository
    {
        Task<BrandEntity?> GetBrandByIdAsync(int id, CancellationToken cancellationToken);
        IQueryable<BrandEntity> GetBrands();
        IQueryable<BrandEntity> GetDeletedBrands();
        IQueryable<BrandEntity> GetAllBrands();
        Task<List<BrandEntity>> GetActiveBrandsByIdsAsync(List<int> ids, CancellationToken cancellationToken);
        Task<List<BrandEntity>> GetDeletedBrandsByIdsAsync(List<int> ids, CancellationToken cancellationToken);
        Task<List<BrandEntity>> GetAllBrandsByIdsAsync(List<int> ids, CancellationToken cancellationToken);
    }
}
