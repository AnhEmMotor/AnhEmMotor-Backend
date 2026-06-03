using ServiceCategoryEntity = Domain.Entities.ServiceCategory;

namespace Application.Interfaces.Repositories.ServiceCategory
{
    public interface IServiceCategoryUpdateRepository
    {
        void Update(ServiceCategoryEntity serviceCategory);
    }
}
