using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.DebtPayments.Queries.GetSuppliersWithDebt
{
    public class GetSuppliersWithDebtQuery : IRequest<Result<List<SupplierDebtResponse>>>
    {
    }
}
