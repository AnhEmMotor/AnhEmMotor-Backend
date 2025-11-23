using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandDeleteRepository
    {
        void Delete(BrandEntity brand);
        void Delete(IEnumerable<BrandEntity> brands);
    }
}
