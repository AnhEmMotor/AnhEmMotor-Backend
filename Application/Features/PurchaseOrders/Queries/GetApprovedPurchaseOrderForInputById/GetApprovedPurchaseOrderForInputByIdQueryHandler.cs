using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseOrder;
using Domain.Constants;
using Mapster;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseOrders.Queries.GetApprovedPurchaseOrderForInputById
{
    public sealed class GetApprovedPurchaseOrderForInputByIdQueryHandler(IPurchaseOrderReadRepository repository) 
        : IRequestHandler<GetApprovedPurchaseOrderForInputByIdQuery, Result<PurchaseOrderDetailForInputResponse?>>
    {
        public async Task<Result<PurchaseOrderDetailForInputResponse?>> Handle(
            GetApprovedPurchaseOrderForInputByIdQuery request,
            CancellationToken cancellationToken)
        {
            var po = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (po is null || po.Status != PurchaseOrderStatus.Approved)
            {
                return Error.NotFound($"Không tìm thấy đơn chốt mua đã phê duyệt có ID {request.Id}.", "Id");
            }
            return po.Adapt<PurchaseOrderDetailForInputResponse?>();
        }
    }
}
