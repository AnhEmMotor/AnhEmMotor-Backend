using Application.Interfaces.Repositories.PurchaseRequest;
using Infrastructure.DBContexts;
using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;
using PurchaseRequestItemEntity = Domain.Entities.PurchaseRequestItem;

namespace Infrastructure.Repositories.PurchaseRequest
{
    public class PurchaseRequestDeleteRepository(ApplicationDBContext context) : IPurchaseRequestDeleteRepository
    {
        public void Delete(PurchaseRequestEntity purchaseRequest)
        {
            context.SoftDeleteUsingSetColumn(purchaseRequest);
        }

        public void DeleteItem(PurchaseRequestItemEntity item)
        {
            context.PurchaseRequestItems.Remove(item);
        }
    }
}
