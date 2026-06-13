using ServiceCategoryEntity = Domain.Entities.ServiceCategory;

namespace Application.Interfaces.Repositories.ServiceCategory
{
    public interface IServiceCategoryUpdateRepository
    {
        public void Update(ServiceCategoryEntity serviceCategory);
    }
}
