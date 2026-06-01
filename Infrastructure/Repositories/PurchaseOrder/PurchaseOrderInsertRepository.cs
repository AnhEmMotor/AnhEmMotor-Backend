using Application.Interfaces.Repositories.PurchaseOrder;
using Infrastructure.DBContexts;
using PurchaseOrderEntity = Domain.Entities.PurchaseOrder;

namespace Infrastructure.Repositories.PurchaseOrder
{
    public class PurchaseOrderInsertRepository(ApplicationDBContext context) : IPurchaseOrderInsertRepository
    {
        public void Add(PurchaseOrderEntity purchaseOrder)
        {
            context.PurchaseOrders.Add(purchaseOrder);
        }
    }
}
