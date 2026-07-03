using Application.Interfaces.Repositories.MaintenanceHistory;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.MaintenanceHistory
{
    public class MaintenanceHistoryUpdateRepository(ApplicationDBContext context) : IMaintenanceHistoryUpdateRepository
    {
        public void Add(Domain.Entities.MaintenanceHistory maintenanceHistory)
        {
            context.MaintenanceHistories.Add(maintenanceHistory);
        }

        public void Update(Domain.Entities.MaintenanceHistory maintenanceHistory)
        {
            context.MaintenanceHistories.Update(maintenanceHistory);
        }

        public void Remove(Domain.Entities.MaintenanceHistory maintenanceHistory)
        {
            context.MaintenanceHistories.Remove(maintenanceHistory);
        }
    }
}
