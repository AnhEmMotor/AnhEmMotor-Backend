using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DebtPayments.Queries.GetReceiptsWithDebtBySupplierId
{
    public sealed class GetReceiptsWithDebtBySupplierIdQueryHandler : IRequestHandler<GetReceiptsWithDebtBySupplierIdQuery, Result<List<InventoryReceiptDebtLineResponse>>>
    {
        public Task<Result<List<InventoryReceiptDebtLineResponse>>> Handle(
            GetReceiptsWithDebtBySupplierIdQuery request,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
