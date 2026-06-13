using Application.Interfaces.Repositories.ServiceCategory;
using Infrastructure.DBContexts;
using ServiceCategoryEntity = Domain.Entities.ServiceCategory;

namespace Infrastructure.Repositories.ServiceCategory;

public class ServiceCategoryUpdateRepository(ApplicationDBContext context) : IServiceCategoryUpdateRepository
{
    public void Update(ServiceCategoryEntity serviceCategory)
    {
        context.ServiceCategories.Update(serviceCategory);
    }
}
