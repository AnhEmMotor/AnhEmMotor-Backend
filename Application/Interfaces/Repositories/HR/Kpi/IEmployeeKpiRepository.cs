using Domain.Entities;

namespace Application.Interfaces.Repositories.HR.Kpi;

public interface IEmployeeKpiRepository
{
    Task<List<KPI>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<KPI?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> HasDuplicateAsync(
        int employeeProfileId,
        string metricName,
        DateTime periodStart,
        DateTime periodEnd,
        int? excludeId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(KPI kpi, CancellationToken cancellationToken = default);

    void Update(KPI kpi);

    void Delete(KPI kpi);
}
