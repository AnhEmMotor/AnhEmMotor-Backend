using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandDeleteRepository
    {
        public void Delete(BrandEntity brand);

        public void Delete(IEnumerable<BrandEntity> brands);
    }
}
