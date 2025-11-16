using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandUpdateRepository
    {
        Task UpdateBrandAsync(BrandEntity brand, CancellationToken cancellationToken);
        Task RestoreBrandAsync(BrandEntity brand, CancellationToken cancellationToken);
        Task RestoreBrandsAsync(List<BrandEntity> brands, CancellationToken cancellationToken);
    }
}
