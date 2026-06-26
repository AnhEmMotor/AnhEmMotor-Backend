using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using Domain.Primitives;
using Mapster;
using MediatR;
using Sieve.Services;
using System.Linq;

namespace Application.Features.DebtPayments.Queries.GetDebtLogsMissingProofs;

public class GetDebtLogsMissingProofsQueryHandler(
    Application.Interfaces.Repositories.SupplierDebt.ISupplierDebtReadRepository supplierDebtReadRepository,
    ISieveProcessor sieveProcessor) : IRequestHandler<GetDebtLogsMissingProofsQuery, Result<PagedResult<SupplierDebtLogResponse>>>
{
    public Task<Result<PagedResult<SupplierDebtLogResponse>>> Handle(
        GetDebtLogsMissingProofsQuery request,
        CancellationToken cancellationToken)
    {
        var query = supplierDebtReadRepository.GetDebtLogsMissingProofsQueryable();

        var totalCount = sieveProcessor.Apply(request.SieveModel, query, applyPagination: false, applySorting: false).Count();
        
        // Add default sorting by PaymentDate desc if not provided
        if (string.IsNullOrEmpty(request.SieveModel.Sorts))
        {
            query = query.OrderByDescending(x => x.PaymentDate);
        }
        
        var logs = sieveProcessor.Apply(request.SieveModel, query).ToList();
        
        var responseLogs = logs.Select(log => 
        {
            var response = log.Adapt<SupplierDebtLogResponse>();
            response.HasProofImage = false; // Query is for missing proofs so it's always false
            return response;
        }).ToList();

        var result = new PagedResult<SupplierDebtLogResponse>(responseLogs, totalCount, request.SieveModel.Page ?? 1, request.SieveModel.PageSize ?? 10);
        return Task.FromResult(Result<PagedResult<SupplierDebtLogResponse>>.Success(result));
    }
}
