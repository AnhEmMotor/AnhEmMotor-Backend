using Domain.Entities;

namespace Application.Interfaces.Repositories.ProductQuotations
{
    public interface IProductQuotationUpdateRepository
    {
        public void Update(ProductQuotation row);
    }
}
