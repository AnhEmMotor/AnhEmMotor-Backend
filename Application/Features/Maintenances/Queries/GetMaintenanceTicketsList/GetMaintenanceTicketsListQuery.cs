using Application.ApiContracts.Maintenance.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Maintenances.Queries.GetMaintenanceTicketsList
{
    public class GetMaintenanceTicketsListQuery : IRequest<Result<PagedResult<MaintenanceTicketResponse>>>
    {
        public SieveModel? SieveModel { get; set; }
    }
}
