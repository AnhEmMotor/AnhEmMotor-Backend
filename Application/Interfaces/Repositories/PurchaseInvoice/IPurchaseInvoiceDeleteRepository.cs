using PurchaseInvoiceEntity = Domain.Entities.PurchaseInvoice;
using PurchaseInvoiceItemEntity = Domain.Entities.PurchaseInvoiceItem;

namespace Application.Interfaces.Repositories.PurchaseInvoice
{
    public interface IPurchaseInvoiceDeleteRepository
    {
        public void Delete(PurchaseInvoiceEntity purchaseInvoice);
        public void DeleteItem(PurchaseInvoiceItemEntity item);
    }
}
