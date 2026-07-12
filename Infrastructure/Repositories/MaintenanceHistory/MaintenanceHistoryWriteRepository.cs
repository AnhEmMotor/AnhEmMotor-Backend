using Application.Interfaces.Repositories.MaintenanceHistory;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.MaintenanceHistory;

public class MaintenanceHistoryWriteRepository(ApplicationDBContext context) : IMaintenanceHistoryWriteRepository
{
    public void Add(global::Domain.Entities.MaintenanceHistory entity) => context.Set<global::Domain.Entities.MaintenanceHistory>(
        )
        .Add(entity);

    public void Update(global::Domain.Entities.MaintenanceHistory entity) => context.Set<global::Domain.Entities.MaintenanceHistory>(
        )
        .Update(entity);

    public void Delete(global::Domain.Entities.MaintenanceHistory entity) => context.Set<global::Domain.Entities.MaintenanceHistory>(
        )
        .Remove(entity);
}
