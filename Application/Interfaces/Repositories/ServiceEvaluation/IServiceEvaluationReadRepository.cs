using Application.ApiContracts.Evaluation.Responses;
using Application.Common.Models;
using Domain.Primitives;
using Sieve.Models;

namespace Application.Interfaces.Repositories.ServiceEvaluation;

public interface IServiceEvaluationReadRepository
{
    public Task<Domain.Entities.ServiceEvaluation?> GetByIdAsync(int evaluationId, CancellationToken cancellationToken);

    public Task<Result<PagedResult<ServiceEvaluationListRowResponse>>> GetPagedEvaluationsAsync(
        SieveModel sieveModel,
        CancellationToken cancellationToken);

    public Task<ServiceEvaluationDetailResponse> GetEvaluationDetailAsync(
        int evaluationId,
        CancellationToken cancellationToken);
}

