using global::Domain.Entities;

namespace Application.Interfaces.Repositories.WorkshopPayment;

public interface IWorkshopPaymentWriteRepository
{
    public void Add(global::Domain.Entities.WorkshopPayment entity);
    public void Update(global::Domain.Entities.WorkshopPayment entity);
    public void Delete(global::Domain.Entities.WorkshopPayment entity);
}
