using Application.Interfaces.Repositories.PurchaseOrder;
using Infrastructure.DBContexts;
using PurchaseOrderEntity = Domain.Entities.PurchaseOrder;

namespace Infrastructure.Repositories.PurchaseOrder
{
    public class PurchaseOrderUpdateRepository(ApplicationDBContext context) : IPurchaseOrderUpdateRepository
    {
        public void Update(PurchaseOrderEntity purchaseOrder)
        {
            context.PurchaseOrders.Update(purchaseOrder);
        }
    }
}
