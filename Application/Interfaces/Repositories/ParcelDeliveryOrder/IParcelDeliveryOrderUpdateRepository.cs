using ParcelDeliveryOrderEntity = Domain.Entities.Logistics.ParcelDeliveryOrder;
using ParcelDeliveryOrderItemEntity = Domain.Entities.Logistics.ParcelDeliveryOrderItem;

namespace Application.Interfaces.Repositories.ParcelDeliveryOrder;

public interface IParcelDeliveryOrderUpdateRepository
{
    public void Update(ParcelDeliveryOrderEntity parcelDeliveryOrder);
    public void UpdateItem(ParcelDeliveryOrderItemEntity parcelDeliveryOrderItem);
}

