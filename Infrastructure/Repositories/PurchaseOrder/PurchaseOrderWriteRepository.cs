using Application.Interfaces.Repositories.PurchaseOrder;
using Infrastructure.DBContexts;
using PurchaseOrderEntity = Domain.Entities.PurchaseOrder;
using PurchaseOrderItemEntity = Domain.Entities.PurchaseOrderItem;

namespace Infrastructure.Repositories.PurchaseOrder
{
    public class PurchaseOrderWriteRepository(ApplicationDBContext context) : IPurchaseOrderWriteRepository
    {
        public void Add(PurchaseOrderEntity purchaseOrder)
        {
            context.PurchaseOrders.Add(purchaseOrder);
        }

        public void Update(PurchaseOrderEntity purchaseOrder)
        {
            context.PurchaseOrders.Update(purchaseOrder);
        }

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
