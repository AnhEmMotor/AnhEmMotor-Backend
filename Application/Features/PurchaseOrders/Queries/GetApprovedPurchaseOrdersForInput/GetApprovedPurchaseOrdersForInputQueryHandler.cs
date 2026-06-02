using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseOrder;
using Domain.Primitives;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseOrders.Queries.GetApprovedPurchaseOrdersForInput
{
    public sealed class GetApprovedPurchaseOrdersForInputQueryHandler(IPurchaseOrderReadRepository repository) 
        : IRequestHandler<GetApprovedPurchaseOrdersForInputQuery, Result<PagedResult<PurchaseOrderListResponse>>>
    {
        public async Task<Result<PagedResult<PurchaseOrderListResponse>>> Handle(
            GetApprovedPurchaseOrdersForInputQuery request,
            CancellationToken cancellationToken)
        {
            var result = await repository.GetApprovedPagedForInputAsync<PurchaseOrderListResponse>(
                request.SieveModel!,
                cancellationToken)
                .ConfigureAwait(false);
            return result;
        }
    }
}
