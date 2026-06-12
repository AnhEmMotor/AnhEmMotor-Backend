using Application.Interfaces.Repositories.RepairOrder;
using Infrastructure.DBContexts;
using System.Collections.Generic;

namespace Infrastructure.Repositories.RepairOrder
{
    public class RepairOrderUpdateRepository(ApplicationDBContext context) : IRepairOrderUpdateRepository
    {
        public void Add(Domain.Entities.RepairOrder repairOrder)
        {
            context.RepairOrders.Add(repairOrder);
        }

        public void Update(Domain.Entities.RepairOrder repairOrder)
        {
            context.RepairOrders.Update(repairOrder);
        }

        public void Remove(Domain.Entities.RepairOrder repairOrder)
        {
            context.RepairOrders.Remove(repairOrder);
        }

        public void AddDetail(Domain.Entities.RepairOrderDetail detail)
        {
            context.RepairOrderDetails.Add(detail);
        }

        public void RemoveDetail(Domain.Entities.RepairOrderDetail detail)
        {
            context.RepairOrderDetails.Remove(detail);
        }

        public void RemoveDetailsRange(IEnumerable<Domain.Entities.RepairOrderDetail> details)
        {
            context.RepairOrderDetails.RemoveRange(details);
        }
    }
}
