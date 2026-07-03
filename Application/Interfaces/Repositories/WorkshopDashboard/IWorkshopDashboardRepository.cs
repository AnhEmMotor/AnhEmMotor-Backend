using Application.Features.Statistical.DTOs;
using System;

namespace Application.Interfaces.Repositories.WorkshopDashboard;

public interface IWorkshopDashboardRepository
{
    Task<WorkshopDashboardDto> GetOverviewAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken);
}
