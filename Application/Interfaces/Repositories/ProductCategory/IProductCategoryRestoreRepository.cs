namespace Application.Interfaces.Repositories.ProductCategory;

public interface IProductCategoryRestoreRepository
{
    void Restore(Domain.Entities.ProductCategory category);
    void Restores(List<Domain.Entities.ProductCategory> categories);
}
