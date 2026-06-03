using Application.ApiContracts.Evaluation.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ServiceEvaluation;
using MediatR;

namespace Application.Features.ServiceWorkshopEvaluations.Queries.GetServiceWorkshopEvaluationDetail;

public class GetServiceWorkshopEvaluationDetailQueryHandler(
    IServiceEvaluationReadRepository serviceEvaluationReadRepository
) : IRequestHandler<GetServiceWorkshopEvaluationDetailQuery, Result<ServiceEvaluationDetailResponse>>
{
    public async Task<Result<ServiceEvaluationDetailResponse>> Handle(
        GetServiceWorkshopEvaluationDetailQuery request,
        CancellationToken cancellationToken)
    {
        return await serviceEvaluationReadRepository.GetEvaluationDetailAsync(request.EvaluationId, cancellationToken)
            .ConfigureAwait(false);
    }
}

