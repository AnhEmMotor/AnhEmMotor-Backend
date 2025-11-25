using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandUpdateRepository
    {
        void Update(BrandEntity brand);

        void Restore(BrandEntity brand);

        void Restore(IEnumerable<BrandEntity> brands);
    }
}
