using Application.Interfaces.Repositories.PurchaseOrder;
using Infrastructure.DBContexts;
using PurchaseOrderEntity = Domain.Entities.PurchaseOrder;
using PurchaseOrderItemEntity = Domain.Entities.PurchaseOrderItem;

namespace Infrastructure.Repositories.PurchaseOrder
{
    public class PurchaseOrderDeleteRepository(ApplicationDBContext context) : IPurchaseOrderDeleteRepository
    {
        public void Delete(PurchaseOrderEntity purchaseOrder)
        {
            context.SoftDeleteUsingSetColumn(purchaseOrder);
        }

        public void DeleteItem(PurchaseOrderItemEntity item)
        {
            context.PurchaseOrderItems.Remove(item);
        }
    }
}
