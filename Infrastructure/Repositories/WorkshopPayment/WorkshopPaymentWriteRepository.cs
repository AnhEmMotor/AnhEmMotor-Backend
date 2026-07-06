using Application.Interfaces.Repositories.WorkshopPayment;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.WorkshopPayment;

public class WorkshopPaymentWriteRepository(
    ApplicationDBContext context) : IWorkshopPaymentWriteRepository
{
    public void Add(global::Domain.Entities.WorkshopPayment entity) => context.Set<global::Domain.Entities.WorkshopPayment>().Add(entity);
    public void Update(global::Domain.Entities.WorkshopPayment entity) => context.Set<global::Domain.Entities.WorkshopPayment>().Update(entity);
    public void Delete(global::Domain.Entities.WorkshopPayment entity) => context.Set<global::Domain.Entities.WorkshopPayment>().Remove(entity);
}
