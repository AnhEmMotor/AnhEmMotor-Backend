using Application.ApiContracts.Evaluation.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ServiceEvaluation;
using MediatR;
using System;

namespace Application.Features.ServiceWorkshopEvaluations.Queries.GetServiceWorkshopEvaluationDetail;

public class GetServiceWorkshopEvaluationDetailQueryHandler(
    IServiceEvaluationReadRepository serviceEvaluationReadRepository
) : IRequestHandler<GetServiceWorkshopEvaluationDetailQuery, Result<ServiceEvaluationDetailResponse>>
{
    public async Task<Result<ServiceEvaluationDetailResponse>> Handle(
        GetServiceWorkshopEvaluationDetailQuery request,
        CancellationToken cancellationToken)
    {
        var detail = await serviceEvaluationReadRepository
                       .GetEvaluationDetailAsync(request.EvaluationId, cancellationToken)
            .ConfigureAwait(false);
        return Result<ServiceEvaluationDetailResponse>.Success(detail);
    }
}
