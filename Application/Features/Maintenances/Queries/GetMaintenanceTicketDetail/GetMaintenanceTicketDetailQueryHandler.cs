using Application.ApiContracts.Maintenance.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.MaintenanceHistory;
using Mapster;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Maintenances.Queries.GetMaintenanceTicketDetail
{
    public class GetMaintenanceTicketDetailQueryHandler(
        IMaintenanceHistoryReadRepository repository) : IRequestHandler<GetMaintenanceTicketDetailQuery, Result<MaintenanceTicketDetailResponse>>
    {
        public async Task<Result<MaintenanceTicketDetailResponse>> Handle(
            GetMaintenanceTicketDetailQuery request,
            CancellationToken cancellationToken)
        {
            var ticket = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (ticket == null)
            {
                return Result<MaintenanceTicketDetailResponse>.Failure("Không tìm thấy phiếu bảo trì.");
            }

            var response = ticket.Adapt<MaintenanceTicketDetailResponse>();
            return Result<MaintenanceTicketDetailResponse>.Success(response);
        }
    }
}
