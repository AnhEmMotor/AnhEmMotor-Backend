using Domain.Entities;

namespace Application.Interfaces.Repositories.ProductQuotations
{
    public interface IProductQuotationDeleteRepository
    {
        void Delete(ProductQuotation row);
    }
}
