using Application.Features.Statistical.DTOs;
using Application.Interfaces.Repositories.WorkshopDashboard;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Statistical.Queries.GetWorkshopDashboard;

public class GetWorkshopDashboardQueryHandler : IRequestHandler<GetWorkshopDashboardQuery, WorkshopDashboardDto>
{
    private readonly IWorkshopDashboardRepository _repository;

    public GetWorkshopDashboardQueryHandler(IWorkshopDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<WorkshopDashboardDto> Handle(GetWorkshopDashboardQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetOverviewAsync(request.FromDate, request.ToDate, cancellationToken);
    }
}
