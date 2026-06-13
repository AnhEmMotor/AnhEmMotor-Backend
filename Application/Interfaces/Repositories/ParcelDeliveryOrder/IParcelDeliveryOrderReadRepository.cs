using ParcelDeliveryOrderEntity = Domain.Entities.Logistics.ParcelDeliveryOrder;

namespace Application.Interfaces.Repositories.ParcelDeliveryOrder;

public interface IParcelDeliveryOrderReadRepository
{
    public Task<List<ParcelDeliveryOrderEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<List<ParcelDeliveryOrderEntity>> GetReturnedAsync(CancellationToken cancellationToken = default);
    public Task<ParcelDeliveryOrderEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    public Task<ParcelDeliveryOrderEntity?> FindByTrackingOrPhoneAsync(string? search, CancellationToken cancellationToken = default);
}

