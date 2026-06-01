using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptsBySupplierId
{
    public sealed class GetInventoryReceiptsBySupplierIdQueryHandler : IRequestHandler<GetInventoryReceiptsBySupplierIdQuery, Result<PagedResult<InventoryReceiptListResponse>>>
    {
        public Task<Result<PagedResult<InventoryReceiptListResponse>>> Handle(
            GetInventoryReceiptsBySupplierIdQuery request,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
