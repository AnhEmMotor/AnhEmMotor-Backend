using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.DebtPayments.Queries.GetReceiptsWithDebtBySupplierId
{
    public class GetReceiptsWithDebtBySupplierIdQuery : IRequest<Result<List<InventoryReceiptDebtLineResponse>>>
    {
        public int SupplierId { get; set; }
    }
}
