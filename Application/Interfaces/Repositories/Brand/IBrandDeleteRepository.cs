using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandDeleteRepository
    {
        Task DeleteBrandAsync(BrandEntity brand, CancellationToken cancellationToken);
        Task DeleteBrandsAsync(List<BrandEntity> brands, CancellationToken cancellationToken);
    }
}
