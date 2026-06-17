using QuotationEntity = Domain.Entities.Quotation;

namespace Application.Interfaces.Repositories.Quotation
{
    public interface IQuotationInsertRepository
    {
        public void Add(QuotationEntity quotation);
    }
}
