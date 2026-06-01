using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptsBySupplierId
{
    public sealed class GetSupplierPurchaseHistoryQueryHandler : IRequestHandler<GetSupplierPurchaseHistoryQuery, Result<PagedResult<SupplierPurchaseHistoryResponse>>>
    {
        public Task<Result<PagedResult<SupplierPurchaseHistoryResponse>>> Handle(
            GetSupplierPurchaseHistoryQuery request,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
