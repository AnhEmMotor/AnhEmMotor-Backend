using Application.Common.Models;
using Domain.Constants.Order;
using MediatR;

namespace Application.Features.Outputs.Queries.GetVehicleAssignmentStatuses;

public sealed class GetVehicleAssignmentStatusesQueryHandler : IRequestHandler<GetVehicleAssignmentStatusesQuery, Result<IEnumerable<string>>>
{
    public Task<Result<IEnumerable<string>>> Handle(
        GetVehicleAssignmentStatusesQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = OrderVehicleAssignmentStatus.ReturnVehicleAssignmentStatus();
        return Task.FromResult(Result<IEnumerable<string>>.Success(result));
    }
}
