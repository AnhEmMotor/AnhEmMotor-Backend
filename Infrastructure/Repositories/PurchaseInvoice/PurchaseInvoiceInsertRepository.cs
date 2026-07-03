using Application.Interfaces.Repositories.PurchaseInvoice;
using Infrastructure.DBContexts;
using PurchaseInvoiceEntity = Domain.Entities.PurchaseInvoice;

namespace Infrastructure.Repositories.PurchaseInvoice
{
    public class PurchaseInvoiceInsertRepository(ApplicationDBContext context) : IPurchaseInvoiceInsertRepository
    {
        public void Add(PurchaseInvoiceEntity purchaseInvoice) => context.Set<PurchaseInvoiceEntity>()
            .Add(purchaseInvoice);
    }

    public class PurchaseInvoiceUpdateRepository(ApplicationDBContext context) : IPurchaseInvoiceUpdateRepository
    {
        public void Update(PurchaseInvoiceEntity purchaseInvoice) => context.Set<PurchaseInvoiceEntity>()
            .Update(purchaseInvoice);
    }

    public class PurchaseInvoiceDeleteRepository(ApplicationDBContext context) : IPurchaseInvoiceDeleteRepository
    {
        public void Delete(PurchaseInvoiceEntity purchaseInvoice) => context.Set<PurchaseInvoiceEntity>()
            .Remove(purchaseInvoice);
    }
}
