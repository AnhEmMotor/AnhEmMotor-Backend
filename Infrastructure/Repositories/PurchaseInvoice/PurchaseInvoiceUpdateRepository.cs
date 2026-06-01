using Application.Interfaces.Repositories.PurchaseInvoice;
using Infrastructure.DBContexts;
using PurchaseInvoiceEntity = Domain.Entities.PurchaseInvoice;

namespace Infrastructure.Repositories.PurchaseInvoice
{
    public class PurchaseInvoiceUpdateRepository(ApplicationDBContext context) : IPurchaseInvoiceUpdateRepository
    {
        public void Update(PurchaseInvoiceEntity purchaseInvoice)
        {
            context.PurchaseInvoices.Update(purchaseInvoice);
        }
    }
}
