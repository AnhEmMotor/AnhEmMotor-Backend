using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;

namespace Application.Interfaces.Repositories.PurchaseRequest
{
    public interface IPurchaseRequestUpdateRepository
    {
        public void Update(PurchaseRequestEntity purchaseRequest);
    }
}
