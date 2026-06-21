using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.DebtPayments.Queries.GetSuppliersWithDebt
{
    public class GetSuppliersWithDebtQuery : IRequest<Result<PagedResult<SupplierDebtResponse>>>
    {
        public SieveModel? SieveModel { get; init; }
    }
}
