using Application.Interfaces.Repositories.PurchaseInvoice;
using Infrastructure.DBContexts;
using PurchaseInvoiceEntity = Domain.Entities.PurchaseInvoice;
using PurchaseInvoiceItemEntity = Domain.Entities.PurchaseInvoiceItem;

namespace Infrastructure.Repositories.PurchaseInvoice
{
    public class PurchaseInvoiceDeleteRepository(ApplicationDBContext context) : IPurchaseInvoiceDeleteRepository
    {
        public void Delete(PurchaseInvoiceEntity purchaseInvoice)
        {
            context.SoftDeleteUsingSetColumn(purchaseInvoice);
        }

        public void DeleteItem(PurchaseInvoiceItemEntity item)
        {
            context.PurchaseInvoiceItems.Remove(item);
        }
    }
}
