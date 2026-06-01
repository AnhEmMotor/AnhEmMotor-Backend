using PurchaseOrderEntity = Domain.Entities.PurchaseOrder;
using PurchaseOrderItemEntity = Domain.Entities.PurchaseOrderItem;

namespace Application.Interfaces.Repositories.PurchaseOrder
{
    public interface IPurchaseOrderDeleteRepository
    {
        public void Delete(PurchaseOrderEntity purchaseOrder);
        public void DeleteItem(PurchaseOrderItemEntity item);
    }
}
