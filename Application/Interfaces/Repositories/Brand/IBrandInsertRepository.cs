using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandInsertRepository
    {
        void Add(BrandEntity brand);
    }
}
