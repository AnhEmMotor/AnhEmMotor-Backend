using Application.Interfaces.Repositories.Quotation;
using Infrastructure.DBContexts;
using QuotationEntity = Domain.Entities.Quotation;

namespace Infrastructure.Repositories.Quotation
{
    public class QuotationInsertRepository(ApplicationDBContext context) : IQuotationInsertRepository
    {
        public void Add(QuotationEntity quotation)
        {
            context.Quotations.Add(quotation);
        }
    }
}
