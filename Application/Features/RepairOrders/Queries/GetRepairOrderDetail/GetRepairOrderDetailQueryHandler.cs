using Application.ApiContracts.RepairOrder.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.RepairOrder;
using Mapster;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.RepairOrders.Queries.GetRepairOrderDetail
{
    public class GetRepairOrderDetailQueryHandler(
        IRepairOrderReadRepository repairOrderReadRepository) : IRequestHandler<GetRepairOrderDetailQuery, Result<RepairOrderResponse>>
    {
        public async Task<Result<RepairOrderResponse>> Handle(GetRepairOrderDetailQuery request, CancellationToken cancellationToken)
        {
            var repairOrder = await repairOrderReadRepository.GetByIdAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);

            if (repairOrder == null)
            {
                return Result<RepairOrderResponse>.Failure(Error.NotFound("Phiếu sửa chữa không tồn tại."));
            }

            var response = repairOrder.Adapt<RepairOrderResponse>();
            return Result<RepairOrderResponse>.Success(response);
        }
    }
}
