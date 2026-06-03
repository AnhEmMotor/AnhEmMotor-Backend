using ServiceCategoryEntity = Domain.Entities.ServiceCategory;

namespace Application.Interfaces.Repositories.ServiceCategory
{
    public interface IServiceCategoryInsertRepository
    {
        void Add(ServiceCategoryEntity serviceCategory);
    }
}
