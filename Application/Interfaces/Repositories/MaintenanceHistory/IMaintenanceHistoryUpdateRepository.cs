namespace Application.Interfaces.Repositories.MaintenanceHistory
{
    public interface IMaintenanceHistoryUpdateRepository
    {
        public void Add(Domain.Entities.MaintenanceHistory maintenanceHistory);
        public void Update(Domain.Entities.MaintenanceHistory maintenanceHistory);
        public void Remove(Domain.Entities.MaintenanceHistory maintenanceHistory);
    }
}
