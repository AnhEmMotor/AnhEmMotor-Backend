using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.DebtPayments.Queries.GetDebtLogsMissingProofs;

public class GetDebtLogsMissingProofsQuery : IRequest<Result<PagedResult<SupplierDebtLogResponse>>>
{
    public SieveModel SieveModel { get; set; } = new SieveModel();
}
