using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandSelectRepository
    {
        Task<BrandEntity?> GetBrandByIdAsync(int id);
        IQueryable<BrandEntity> GetBrands();
        IQueryable<BrandEntity> GetDeletedBrands();
        Task<List<BrandEntity>> GetActiveBrandsByIdsAsync(List<int> ids);
        Task<List<BrandEntity>> GetDeletedBrandsByIdsAsync(List<int> ids);
    }
}
