using Application.Interfaces.Repositories.ProductQuotations;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Repositories.ProductQuotations
{
    public class ProductQuotationRepository(ApplicationDBContext context) : IProductQuotationReadRepository, IProductQuotationInsertRepository, IProductQuotationUpdateRepository, IProductQuotationDeleteRepository
    {
        public Task<List<ProductQuotation>> GetByVariantAsync(int variantId, CancellationToken cancellationToken)
        {
            return context.ProductQuotations
                .Include(q => q.Supplier)
                .Where(q => q.ProductVariantId == variantId)
                .ToListAsync(cancellationToken);
        }

        public Task<ProductQuotation?> GetBySupplierAndVariantAsync(
            int variantId,
            int? colorId,
            int supplierId,
            CancellationToken cancellationToken)
        {
            return context.ProductQuotations
                .FirstOrDefaultAsync(
                    q => q.ProductVariantId == variantId &&
                        q.ProductVariantColorId == colorId &&
                        q.SupplierId == supplierId,
                    cancellationToken);
        }

        public Task<List<ProductQuotation>> GetAllAsync(CancellationToken cancellationToken)
        {
            return context.ProductQuotations.ToListAsync(cancellationToken);
        }

        public void Update(ProductQuotation row)
        {
            context.ProductQuotations.Update(row);
        }

        public void Delete(ProductQuotation row)
        {
            context.ProductQuotations.Remove(row);
        }

        public async Task AddAsync(ProductQuotation row, CancellationToken cancellationToken)
        {
            await context.ProductQuotations.AddAsync(row, cancellationToken).ConfigureAwait(false);
        }
    }
}
