using Application.Interfaces.Repositories.PurchaseInvoice;
using Infrastructure.DBContexts;
using PurchaseInvoiceEntity = Domain.Entities.PurchaseInvoice;

namespace Infrastructure.Repositories.PurchaseInvoice
{
    public class PurchaseInvoiceInsertRepository(ApplicationDBContext context) : IPurchaseInvoiceInsertRepository
    {
        public void Add(PurchaseInvoiceEntity purchaseInvoice)
        {
            context.PurchaseInvoices.Add(purchaseInvoice);
        }
    }
}
