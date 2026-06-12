using ParcelDeliveryOrderEntity = Domain.Entities.Logistics.ParcelDeliveryOrder;

namespace Application.Interfaces.Repositories.ParcelDeliveryOrder;

public interface IParcelDeliveryOrderReadRepository
{
    Task<List<ParcelDeliveryOrderEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<ParcelDeliveryOrderEntity>> GetReturnedAsync(CancellationToken cancellationToken = default);
    Task<ParcelDeliveryOrderEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ParcelDeliveryOrderEntity?> FindByTrackingOrPhoneAsync(string? search, CancellationToken cancellationToken = default);
}
