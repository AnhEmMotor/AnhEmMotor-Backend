using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.SupplierDebt;
using Domain.Primitives;
using Mapster;
using MediatR;
using Sieve.Services;
using System.Linq;

namespace Application.Features.DebtPayments.Queries.GetDebtLogsMissingProofs;

public class GetDebtLogsMissingProofsQueryHandler(
    ISupplierDebtReadRepository supplierDebtReadRepository,
    ISieveProcessor sieveProcessor) : IRequestHandler<GetDebtLogsMissingProofsQuery, Result<PagedResult<SupplierDebtLogResponse>>>
{
    public Task<Result<PagedResult<SupplierDebtLogResponse>>> Handle(
        GetDebtLogsMissingProofsQuery request,
        CancellationToken cancellationToken)
    {
        var query = supplierDebtReadRepository.GetDebtLogsMissingProofsQueryable();
        var totalCount = sieveProcessor.Apply(request.SieveModel, query, applyPagination: false, applySorting: false)
            .Count();
        if (string.IsNullOrEmpty(request.SieveModel.Sorts))
        {
            query = query.OrderByDescending(x => x.PaymentDate);
        }
        var logs = sieveProcessor.Apply(request.SieveModel, query).ToList();
        var responseLogs = logs.Select(
            log =>
            {
                var response = log.Adapt<SupplierDebtLogResponse>();
                response.HasProofImage = false;
                return response;
            })
            .ToList();
        var result = new PagedResult<SupplierDebtLogResponse>(
            responseLogs,
            totalCount,
            request.SieveModel.Page ?? 1,
            request.SieveModel.PageSize ?? 10);
        return Task.FromResult(Result<PagedResult<SupplierDebtLogResponse>>.Success(result));
    }
}
