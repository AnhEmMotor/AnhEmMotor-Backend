using System.Collections.Generic;

namespace Application.Interfaces.Repositories.RepairOrder
{
    public interface IRepairOrderUpdateRepository
    {
        void Add(Domain.Entities.RepairOrder repairOrder);
        void Update(Domain.Entities.RepairOrder repairOrder);
        void Remove(Domain.Entities.RepairOrder repairOrder);
        
        void AddDetail(Domain.Entities.RepairOrderDetail detail);
        void RemoveDetail(Domain.Entities.RepairOrderDetail detail);
        void RemoveDetailsRange(IEnumerable<Domain.Entities.RepairOrderDetail> details);
    }
}
