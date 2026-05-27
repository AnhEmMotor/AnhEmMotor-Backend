using System.Collections.Generic;
using QuotationEntity = Domain.Entities.Quotation;

namespace Application.Interfaces.Repositories.Quotation
{
    public interface IQuotationUpdateRepository
    {
        void Update(QuotationEntity quotation);

        void Restore(QuotationEntity quotation);

        void Restore(IEnumerable<QuotationEntity> quotations);
    }
}
