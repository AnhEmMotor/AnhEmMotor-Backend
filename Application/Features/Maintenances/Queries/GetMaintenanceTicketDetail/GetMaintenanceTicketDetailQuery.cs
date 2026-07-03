using Application.ApiContracts.Maintenance.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Maintenances.Queries.GetMaintenanceTicketDetail
{
    public class GetMaintenanceTicketDetailQuery : IRequest<Result<MaintenanceTicketDetailResponse>>
    {
        public int Id { get; set; }
    }
}
