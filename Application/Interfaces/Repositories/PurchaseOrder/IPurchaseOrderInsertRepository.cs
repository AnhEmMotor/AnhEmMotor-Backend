using PurchaseOrderEntity = Domain.Entities.PurchaseOrder;

namespace Application.Interfaces.Repositories.PurchaseOrder
{
    public interface IPurchaseOrderInsertRepository
    {
        public void Add(PurchaseOrderEntity purchaseOrder);
    }
}
