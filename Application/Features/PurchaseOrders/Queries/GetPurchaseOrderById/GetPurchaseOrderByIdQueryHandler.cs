using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseOrder;
using Mapster;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById
{
    public sealed class GetPurchaseOrderByIdQueryHandler(IPurchaseOrderReadRepository repository) : IRequestHandler<GetPurchaseOrderByIdQuery, Result<PurchaseOrderDetailResponse?>>
    {
        public async Task<Result<PurchaseOrderDetailResponse?>> Handle(
            GetPurchaseOrderByIdQuery request,
            CancellationToken cancellationToken)
        {
            var po = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (po is null)
            {
                return Error.NotFound($"Không tìm thấy đơn chốt mua có ID {request.Id}.", "Id");
            }
            return po.Adapt<PurchaseOrderDetailResponse?>();
        }
    }
}
