using Application.Interfaces.Repositories.Quotation;
using Infrastructure.DBContexts;
using QuotationEntity = Domain.Entities.Quotation;

namespace Infrastructure.Repositories.Quotation
{
    public class QuotationDeleteRepository(ApplicationDBContext context) : IQuotationDeleteRepository
    {
        public void Delete(QuotationEntity quotation)
        {
            context.SoftDeleteUsingSetColumn(quotation);
        }

        public void Delete(IEnumerable<QuotationEntity> quotations)
        {
            context.SoftDeleteUsingSetColumnRange(quotations);
        }
    }
}
