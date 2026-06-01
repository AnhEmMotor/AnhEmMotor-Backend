using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseOrder;
using Domain.Primitives;
using Domain.Constants;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseOrders.Queries.GetPurchaseOrders
{
    public sealed class GetPurchaseOrdersQueryHandler(IPurchaseOrderReadRepository repository) : IRequestHandler<GetPurchaseOrdersQuery, Result<PagedResult<PurchaseOrderListResponse>>>
    {
        public async Task<Result<PagedResult<PurchaseOrderListResponse>>> Handle(
            GetPurchaseOrdersQuery request,
            CancellationToken cancellationToken)
        {
            var result = await repository.GetPagedAsync<PurchaseOrderListResponse>(
                request.SieveModel!,
                DataFetchMode.ActiveOnly,
                cancellationToken)
                .ConfigureAwait(false);
            return result;
        }
    }
}
