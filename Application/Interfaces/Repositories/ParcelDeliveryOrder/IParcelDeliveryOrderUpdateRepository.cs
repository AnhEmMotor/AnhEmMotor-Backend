using ParcelDeliveryOrderEntity = Domain.Entities.Logistics.ParcelDeliveryOrder;
using ParcelDeliveryOrderItemEntity = Domain.Entities.Logistics.ParcelDeliveryOrderItem;

namespace Application.Interfaces.Repositories.ParcelDeliveryOrder;

public interface IParcelDeliveryOrderUpdateRepository
{
    void Update(ParcelDeliveryOrderEntity parcelDeliveryOrder);
    void UpdateItem(ParcelDeliveryOrderItemEntity parcelDeliveryOrderItem);
}
