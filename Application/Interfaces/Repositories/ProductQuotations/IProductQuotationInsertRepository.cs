using Domain.Entities;

namespace Application.Interfaces.Repositories.ProductQuotations
{
    public interface IProductQuotationInsertRepository
    {
        public Task AddAsync(ProductQuotation row, CancellationToken cancellationToken);
    }
}
