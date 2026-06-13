using Application.ApiContracts.Evaluation.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ServiceEvaluation;
using Domain.Primitives;
using MediatR;

namespace Application.Features.ServiceWorkshopEvaluations.Queries.GetServiceWorkshopEvaluations;

public class GetServiceWorkshopEvaluationsQueryHandler(
    IServiceEvaluationReadRepository serviceEvaluationReadRepository
) : IRequestHandler<GetServiceWorkshopEvaluationsQuery, Result<PagedResult<ServiceEvaluationListRowResponse>>>
{
    public Task<Result<PagedResult<ServiceEvaluationListRowResponse>>> Handle(
        GetServiceWorkshopEvaluationsQuery request,
        CancellationToken cancellationToken)
    {
        var sieveModel = request.SieveModel ?? new();
        return serviceEvaluationReadRepository.GetPagedEvaluationsAsync(sieveModel, cancellationToken);
    }
}

