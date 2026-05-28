using System.Collections.Generic;
using QuotationEntity = Domain.Entities.Quotation;

namespace Application.Interfaces.Repositories.Quotation
{
    public interface IQuotationDeleteRepository
    {
        public void Delete(QuotationEntity quotation);

        public void Delete(IEnumerable<QuotationEntity> quotations);
    }
}
