using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandUpdateRepository
    {
        public void Update(BrandEntity brand);

        public void Restore(BrandEntity brand);

        public void Restore(IEnumerable<BrandEntity> brands);
    }
}
