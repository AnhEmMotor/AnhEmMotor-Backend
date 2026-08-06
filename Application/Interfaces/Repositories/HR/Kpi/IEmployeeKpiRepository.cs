using Domain.Entities;

namespace Application.Interfaces.Repositories.HR.Kpi;

public interface IEmployeeKpiRepository
{
    public Task<List<KPI>> GetAllAsync(CancellationToken cancellationToken = default);

    public Task<KPI?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    public Task<bool> HasDuplicateAsync(
        int employeeProfileId,
        string metricName,
        DateTime periodStart,
        DateTime periodEnd,
        int? excludeId = null,
        CancellationToken cancellationToken = default);

    public Task AddAsync(KPI kpi, CancellationToken cancellationToken = default);

    public void Update(KPI kpi);

    public void Delete(KPI kpi);
}
