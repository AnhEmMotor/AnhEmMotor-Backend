using QuotationEntity = Domain.Entities.Quotation;

namespace Application.Interfaces.Repositories.Quotation
{
    public interface IQuotationInsertRepository
    {
        void Add(QuotationEntity quotation);
    }
}
