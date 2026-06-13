using ServiceCategoryEntity = Domain.Entities.ServiceCategory;

namespace Application.Interfaces.Repositories.ServiceCategory
{
    public interface IServiceCategoryInsertRepository
    {
        public void Add(ServiceCategoryEntity serviceCategory);
    }
}
