using Application.Interfaces.Repositories.PurchaseRequest;
using Infrastructure.DBContexts;
using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;

namespace Infrastructure.Repositories.PurchaseRequest
{
    public class PurchaseRequestUpdateRepository(ApplicationDBContext context) : IPurchaseRequestUpdateRepository
    {
        public void Update(PurchaseRequestEntity purchaseRequest)
        {
            context.PurchaseRequests.Update(purchaseRequest);
        }

        public void Restore(PurchaseRequestEntity purchaseRequest)
        {
            purchaseRequest.DeletedAt = null;
            context.PurchaseRequests.Update(purchaseRequest);
        }
    }
}
