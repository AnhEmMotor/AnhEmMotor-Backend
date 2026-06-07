using Domain.Entities;

namespace Application.Interfaces.Repositories.ProductQuotations
{
    public interface IProductQuotationDeleteRepository
    {
        public void Delete(ProductQuotation row);
    }
}
