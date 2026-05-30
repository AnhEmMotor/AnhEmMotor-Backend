using Application.Interfaces.Repositories.PurchaseRequest;
using Infrastructure.DBContexts;
using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;

namespace Infrastructure.Repositories.PurchaseRequest
{
    public class PurchaseRequestInsertRepository(ApplicationDBContext context) : IPurchaseRequestInsertRepository
    {
        public void Add(PurchaseRequestEntity purchaseRequest)
        {
            context.PurchaseRequests.Add(purchaseRequest);
        }
    }
}
