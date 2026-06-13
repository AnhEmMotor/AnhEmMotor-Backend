using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Infrastructure.DBContexts;
using ParcelDeliveryOrderEntity = Domain.Entities.Logistics.ParcelDeliveryOrder;
using ParcelDeliveryOrderItemEntity = Domain.Entities.Logistics.ParcelDeliveryOrderItem;

namespace Infrastructure.Repositories.ParcelDeliveryOrder;

public class ParcelDeliveryOrderUpdateRepository(ApplicationDBContext context) : IParcelDeliveryOrderUpdateRepository
{
    public void Update(ParcelDeliveryOrderEntity parcelDeliveryOrder) => context.Set<ParcelDeliveryOrderEntity>()
        .Update(parcelDeliveryOrder);

    public void UpdateItem(ParcelDeliveryOrderItemEntity parcelDeliveryOrderItem) => context.Set<ParcelDeliveryOrderItemEntity>(
        )
        .Update(parcelDeliveryOrderItem);
}
