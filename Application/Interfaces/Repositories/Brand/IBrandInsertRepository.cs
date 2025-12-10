using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandInsertRepository
    {
        public void Add(BrandEntity brand);
    }
}
