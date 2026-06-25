using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;

namespace Application.Interfaces.Repositories.PurchaseRequest
{
    public interface IPurchaseRequestUpdateRepository
    {
        public void Update(PurchaseRequestEntity purchaseRequest);

        public void Restore(PurchaseRequestEntity purchaseRequest);
    }
}
