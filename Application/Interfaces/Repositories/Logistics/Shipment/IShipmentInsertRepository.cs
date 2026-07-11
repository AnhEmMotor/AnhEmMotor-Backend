
namespace Application.Interfaces.Repositories.Logistics.Shipment;

public interface IShipmentInsertRepository
{
    Task AddAsync(Domain.Entities.Logistics.Shipment shipment, CancellationToken cancellationToken = default);
}
