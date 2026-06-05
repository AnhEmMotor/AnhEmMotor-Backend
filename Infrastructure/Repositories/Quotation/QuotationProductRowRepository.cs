using Application.Interfaces.Repositories.Quotation;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Quotation
{
    public class QuotationProductRowRepository(ApplicationDBContext context) : IQuotationProductRowRepository
    {
        public Task<QuotationProductRow?> GetBySupplierAndVariantAsync(
            int variantId,
            int? colorId,
            int supplierId,
            CancellationToken cancellationToken)
        {
            return context.QuotationProductRows
                .FirstOrDefaultAsync(q => q.ProductVariantId == variantId &&
                                         q.ProductVariantColorId == colorId &&
                                         q.SupplierId == supplierId, cancellationToken);
        }

        public void Update(QuotationProductRow row)
        {
            context.QuotationProductRows.Update(row);
        }

        public void Delete(QuotationProductRow row)
        {
            context.QuotationProductRows.Remove(row);
        }

        public async Task AddAsync(QuotationProductRow row, CancellationToken cancellationToken)
        {
            await context.QuotationProductRows.AddAsync(row, cancellationToken);
        }
    }
}
