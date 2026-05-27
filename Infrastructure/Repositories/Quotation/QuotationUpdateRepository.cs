using Application.Interfaces.Repositories.Quotation;
using Infrastructure.DBContexts;
using System.Collections.Generic;
using QuotationEntity = Domain.Entities.Quotation;

namespace Infrastructure.Repositories.Quotation
{
    public class QuotationUpdateRepository(ApplicationDBContext context) : IQuotationUpdateRepository
    {
        public void Update(QuotationEntity quotation)
        {
            context.Quotations.Update(quotation);
        }

        public void Restore(QuotationEntity quotation)
        {
            context.Restore(quotation);
        }

        public void Restore(IEnumerable<QuotationEntity> quotations)
        {
            context.RestoreDeleteUsingSetColumnRange(quotations);
        }
    }
}
