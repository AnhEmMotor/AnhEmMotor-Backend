using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;

namespace Application.Interfaces.Repositories.PurchaseRequest
{
    public interface IPurchaseRequestInsertRepository
    {
        public void Add(PurchaseRequestEntity purchaseRequest);
    }
}
