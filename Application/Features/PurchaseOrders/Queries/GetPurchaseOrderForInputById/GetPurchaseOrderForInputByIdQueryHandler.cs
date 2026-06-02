using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseOrder;
using Mapster;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseOrders.Queries.GetPurchaseOrderForInputById
{
    public sealed class GetPurchaseOrderForInputByIdQueryHandler(IPurchaseOrderReadRepository repository) : IRequestHandler<GetPurchaseOrderForInputByIdQuery, Result<PurchaseOrderDetailForInputResponse?>>
    {
        public async Task<Result<PurchaseOrderDetailForInputResponse?>> Handle(
            GetPurchaseOrderForInputByIdQuery request,
            CancellationToken cancellationToken)
        {
            var po = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (po is null)
            {
                return Error.NotFound($"Không tìm thấy đơn chốt mua có ID {request.Id}.", "Id");
            }
            return po.Adapt<PurchaseOrderDetailForInputResponse?>();
        }
    }
}
