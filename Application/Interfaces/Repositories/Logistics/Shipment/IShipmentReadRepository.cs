
namespace Application.Interfaces.Repositories.Logistics.Shipment;

public interface IShipmentReadRepository
{
    public Task<Domain.Entities.Logistics.Shipment?> GetByOutputIdAsync(
        int outputId,
        CancellationToken cancellationToken = default);

    public Task<Domain.Entities.Logistics.Shipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    public Task<List<Domain.Entities.Logistics.Shipment>> GetAllAsync(CancellationToken cancellationToken = default);

    public Task<List<Domain.Entities.Logistics.Shipment>> GetActiveDeliveryShipmentsAsync(
        CancellationToken cancellationToken = default);

    public Task<Domain.Entities.Logistics.Shipment?> GetByTrackingNumberAsync(
        string trackingNumber,
        CancellationToken cancellationToken = default);
}
