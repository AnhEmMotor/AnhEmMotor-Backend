using PurchaseInvoiceEntity = Domain.Entities.PurchaseInvoice;

namespace Application.Interfaces.Repositories.PurchaseInvoice
{
    public interface IPurchaseInvoiceUpdateRepository
    {
        public void Update(PurchaseInvoiceEntity purchaseInvoice);
    }
}
