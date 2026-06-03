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
    public async Task<Result<PagedResult<ServiceEvaluationListRowResponse>>> Handle(
        GetServiceWorkshopEvaluationsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await serviceEvaluationReadRepository.GetPagedEvaluationsAsync(
            new GetServiceWorkshopEvaluationsFilter(
                request.Status,
                request.Criteria,
                request.Search,
                request.Page,
                request.PageSize),
            cancellationToken);

        return result;
    }
}

public record GetServiceWorkshopEvaluationsFilter(
    string? Status,
    string? Criteria,
    string? Search,
    int Page,
    int PageSize);

