using Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.Quotation
{
    public interface IQuotationProductRowRepository
    {
        Task<QuotationProductRow?> GetBySupplierAndVariantAsync(
            int variantId,
            int? colorId,
            int supplierId,
            CancellationToken cancellationToken);

        void Update(QuotationProductRow row);

        void Delete(QuotationProductRow row);

        Task AddAsync(QuotationProductRow row, CancellationToken cancellationToken);
    }
}
