using System.Linq;
using BrandEntity = Domain.Entities.Brand;
using CategoryEntity = Domain.Entities.ProductCategory;
using OptionEntity = Domain.Entities.Option;
using OptionValueEntity = Domain.Entities.OptionValue;
using ProductEntity = Domain.Entities.Product;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Interfaces.Repositories.Product;

public interface IProductSelectRepository
{
    Task<ProductEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<ProductEntity?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken);
    Task<ProductEntity?> GetProductWithDetailsByIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken);
    Task<ProductVariantEntity?> GetVariantBySlugAsync(string slug, bool includeDeleted, CancellationToken cancellationToken);
    IQueryable<ProductEntity> GetProducts();
    IQueryable<ProductEntity> GetActiveProducts();
    IQueryable<ProductVariantEntity> GetActiveVariants();
    IQueryable<ProductEntity> GetDeletedProducts();
    IQueryable<ProductVariantEntity> GetDeletedVariants();
    IQueryable<ProductEntity> GetAllProducts();
    Task<List<ProductEntity>> GetActiveProductsByIdsAsync(List<int> ids, CancellationToken cancellationToken);
    Task<List<ProductEntity>> GetDeletedProductsByIdsAsync(List<int> ids, CancellationToken cancellationToken);
    Task<List<ProductEntity>> GetAllProductsByIdsAsync(List<int> ids, CancellationToken cancellationToken);
    Task<List<ProductVariantEntity>> GetVariantsByProductIdAsync(int productId, CancellationToken cancellationToken);
    Task<CategoryEntity?> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken);
    Task<BrandEntity?> GetBrandByIdAsync(int brandId, CancellationToken cancellationToken);
    Task<OptionEntity?> GetOptionByIdAsync(int optionId, CancellationToken cancellationToken);
    Task<List<OptionEntity>> GetOptionsByIdsAsync(List<int> optionIds, CancellationToken cancellationToken);
    Task<List<OptionValueEntity>> GetOptionValuesByIdsAsync(List<int> optionValueIds, CancellationToken cancellationToken);
    Task<OptionValueEntity?> GetOptionValueByNameAsync(int optionId, string name, CancellationToken cancellationToken);
    Task<bool> OptionValuesBelongToOptionsAsync(List<int> optionValueIds, List<int> optionIds, CancellationToken cancellationToken);
}
