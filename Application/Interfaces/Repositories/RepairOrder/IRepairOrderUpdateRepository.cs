using Domain.Entities;

namespace Application.Interfaces.Repositories.RepairOrder
{
    public interface IRepairOrderUpdateRepository
    {
        public void Add(Domain.Entities.RepairOrder repairOrder);

        public void Update(Domain.Entities.RepairOrder repairOrder);

        public void Remove(Domain.Entities.RepairOrder repairOrder);

        public void AddDetail(RepairOrderDetail detail);

        public void RemoveDetail(RepairOrderDetail detail);

        public void RemoveDetailsRange(IEnumerable<RepairOrderDetail> details);
    }
}
