using global::Domain.Entities;

namespace Application.Interfaces.Repositories.MaintenanceHistory;

public interface IMaintenanceHistoryWriteRepository
{
    public void Add(global::Domain.Entities.MaintenanceHistory entity);
    public void Update(global::Domain.Entities.MaintenanceHistory entity);
    public void Delete(global::Domain.Entities.MaintenanceHistory entity);
}
