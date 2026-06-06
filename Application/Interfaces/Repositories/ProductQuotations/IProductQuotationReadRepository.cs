using Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.ProductQuotations
{
    public interface IProductQuotationReadRepository
    {
        Task<List<ProductQuotation>> GetByVariantAsync(
            int variantId,
            CancellationToken cancellationToken);

        Task<ProductQuotation?> GetBySupplierAndVariantAsync(
            int variantId,
            int? colorId,
            int supplierId,
            CancellationToken cancellationToken);
    }
}
