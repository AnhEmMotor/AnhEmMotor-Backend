using Domain.Entities;

namespace Application.Interfaces.Repositories.ProductQuotations
{
    public interface IProductQuotationReadRepository
    {
        public Task<List<ProductQuotation>> GetByVariantAsync(int variantId, CancellationToken cancellationToken);

        public Task<ProductQuotation?> GetBySupplierAndVariantAsync(
            int variantId,
            int? colorId,
            int supplierId,
            CancellationToken cancellationToken);
    }
}
