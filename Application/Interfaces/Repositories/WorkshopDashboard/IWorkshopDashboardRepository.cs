using Application.Features.Statistical.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.WorkshopDashboard;

public interface IWorkshopDashboardRepository
{
    Task<WorkshopDashboardDto> GetOverviewAsync(DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken);
}
