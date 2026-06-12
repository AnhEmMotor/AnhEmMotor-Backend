using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using ParcelDeliveryOrderEntity = Domain.Entities.Logistics.ParcelDeliveryOrder;
using ParcelDeliveryOrderItemEntity = Domain.Entities.Logistics.ParcelDeliveryOrderItem;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.ParcelDeliveryOrder;

public class ParcelDeliveryOrderUpdateRepository(ApplicationDBContext context) : IParcelDeliveryOrderUpdateRepository
{
    public void Update(ParcelDeliveryOrderEntity parcelDeliveryOrder) => context.Set<ParcelDeliveryOrderEntity>().Update(parcelDeliveryOrder);
    public void UpdateItem(ParcelDeliveryOrderItemEntity parcelDeliveryOrderItem) => context.Set<ParcelDeliveryOrderItemEntity>().Update(parcelDeliveryOrderItem);
}
