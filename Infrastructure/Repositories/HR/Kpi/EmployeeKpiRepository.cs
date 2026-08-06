using Application.Interfaces.Repositories.HR.Kpi;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.HR.Kpi;

public sealed class EmployeeKpiRepository(ApplicationDBContext context) : IEmployeeKpiRepository
{
    public Task<List<KPI>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return context.KPIs
            .AsNoTracking()
            .Include(kpi => kpi.EmployeeProfile)
            .ThenInclude(employee => employee.User)
            .OrderByDescending(kpi => kpi.PeriodEnd)
            .ThenBy(kpi => kpi.EmployeeProfile.User.FullName)
            .ToListAsync(cancellationToken);
    }

    public Task<KPI?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return context.KPIs
            .Include(kpi => kpi.EmployeeProfile)
            .ThenInclude(employee => employee.User)
            .FirstOrDefaultAsync(kpi => kpi.Id == id, cancellationToken);
    }

    public Task<bool> HasDuplicateAsync(
        int employeeProfileId,
        string metricName,
        DateTime periodStart,
        DateTime periodEnd,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedMetricName = metricName.Trim();
        return context.KPIs
            .AnyAsync(
                kpi => kpi.EmployeeProfileId == employeeProfileId &&
                    kpi.MetricName == normalizedMetricName &&
                    kpi.PeriodStart == periodStart &&
                    kpi.PeriodEnd == periodEnd &&
                    (!excludeId.HasValue || kpi.Id != excludeId.Value),
                cancellationToken);
    }

    public Task AddAsync(KPI kpi, CancellationToken cancellationToken = default)
    {
        return context.KPIs.AddAsync(kpi, cancellationToken).AsTask();
    }

    public void Update(KPI kpi)
    {
        context.KPIs.Update(kpi);
    }

    public void Delete(KPI kpi)
    {
        context.KPIs.Remove(kpi);
    }
}
