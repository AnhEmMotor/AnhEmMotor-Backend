using Application.Interfaces.Repositories.ProductCategory;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.ProductCategory;

public class ProductCategoryRestoreRepository(ApplicationDBContext context) : IProductCategoryRestoreRepository
{
    public void Restore(Domain.Entities.ProductCategory category)
    {
        context.Restore(category);
    }

    public void Restores(List<Domain.Entities.ProductCategory> categories)
    {
        foreach (var category in categories)
        {
            context.Restore(category);
        }
    }
}
