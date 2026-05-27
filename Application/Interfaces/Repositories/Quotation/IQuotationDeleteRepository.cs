using System.Collections.Generic;
using QuotationEntity = Domain.Entities.Quotation;

namespace Application.Interfaces.Repositories.Quotation
{
    public interface IQuotationDeleteRepository
    {
        void Delete(QuotationEntity quotation);

        void Delete(IEnumerable<QuotationEntity> quotations);
    }
}
