using System.Collections.Generic;

namespace Application.Interfaces.Repositories.RepairOrder
{
    public interface IRepairOrderUpdateRepository
    {
        public void Add(Domain.Entities.RepairOrder repairOrder);
        public void Update(Domain.Entities.RepairOrder repairOrder);
        public void Remove(Domain.Entities.RepairOrder repairOrder);
        
        public void AddDetail(Domain.Entities.RepairOrderDetail detail);
        public void RemoveDetail(Domain.Entities.RepairOrderDetail detail);
        public void RemoveDetailsRange(IEnumerable<Domain.Entities.RepairOrderDetail> details);
    }
}
