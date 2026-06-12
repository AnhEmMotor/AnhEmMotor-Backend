using Application.ApiContracts.Evaluation.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.ServiceWorkshopEvaluations.Queries.GetServiceWorkshopEvaluationDetail;

public class GetServiceWorkshopEvaluationDetailQuery : IRequest<Result<ServiceEvaluationDetailResponse>>
{
    public int EvaluationId { get; init; }
}

