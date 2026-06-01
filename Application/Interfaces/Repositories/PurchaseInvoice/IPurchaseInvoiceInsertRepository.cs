using PurchaseInvoiceEntity = Domain.Entities.PurchaseInvoice;

namespace Application.Interfaces.Repositories.PurchaseInvoice
{
    public interface IPurchaseInvoiceInsertRepository
    {
        public void Add(PurchaseInvoiceEntity purchaseInvoice);
    }
}
