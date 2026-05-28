using System.Collections.Generic;
using QuotationEntity = Domain.Entities.Quotation;

namespace Application.Interfaces.Repositories.Quotation
{
    public interface IQuotationUpdateRepository
    {
        public void Update(QuotationEntity quotation);

        public void Restore(QuotationEntity quotation);

        public void Restore(IEnumerable<QuotationEntity> quotations);
    }
}
