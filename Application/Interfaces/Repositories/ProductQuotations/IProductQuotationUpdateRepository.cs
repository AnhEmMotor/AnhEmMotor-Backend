using Domain.Entities;

namespace Application.Interfaces.Repositories.ProductQuotations
{
    public interface IProductQuotationUpdateRepository
    {
        void Update(ProductQuotation row);
    }
}
