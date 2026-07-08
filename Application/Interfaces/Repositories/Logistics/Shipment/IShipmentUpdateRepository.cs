using Domain.Entities.Logistics;

namespace Application.Interfaces.Repositories.Logistics.Shipment;

public interface IShipmentUpdateRepository
{
    void Update(Domain.Entities.Logistics.Shipment shipment);
}
