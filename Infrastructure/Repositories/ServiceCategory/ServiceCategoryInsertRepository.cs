using Application.Interfaces.Repositories.ServiceCategory;
using Infrastructure.DBContexts;
using ServiceCategoryEntity = Domain.Entities.ServiceCategory;

namespace Infrastructure.Repositories.ServiceCategory;

public class ServiceCategoryInsertRepository(ApplicationDBContext context) : IServiceCategoryInsertRepository
{
    public void Add(ServiceCategoryEntity serviceCategory)
    {
        context.ServiceCategories.Add(serviceCategory);
    }
}
