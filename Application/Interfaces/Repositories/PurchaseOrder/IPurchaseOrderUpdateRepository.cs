using PurchaseOrderEntity = Domain.Entities.PurchaseOrder;

namespace Application.Interfaces.Repositories.PurchaseOrder
{
    public interface IPurchaseOrderUpdateRepository
    {
        public void Update(PurchaseOrderEntity purchaseOrder);
    }
}
