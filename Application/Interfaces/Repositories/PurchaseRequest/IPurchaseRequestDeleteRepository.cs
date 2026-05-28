using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;
using PurchaseRequestItemEntity = Domain.Entities.PurchaseRequestItem;

namespace Application.Interfaces.Repositories.PurchaseRequest
{
    public interface IPurchaseRequestDeleteRepository
    {
        public void Delete(PurchaseRequestEntity purchaseRequest);
        public void DeleteItem(PurchaseRequestItemEntity item);
    }
}
