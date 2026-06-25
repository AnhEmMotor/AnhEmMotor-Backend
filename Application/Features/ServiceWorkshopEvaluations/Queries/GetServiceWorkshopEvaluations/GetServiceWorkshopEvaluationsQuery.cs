using Application.ApiContracts.Evaluation.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;

namespace Application.Features.ServiceWorkshopEvaluations.Queries.GetServiceWorkshopEvaluations;

public class GetServiceWorkshopEvaluationsQuery : IRequest<Result<PagedResult<ServiceEvaluationListRowResponse>>>
{
    public string? Status { get; init; }

    public string? Criteria { get; init; }

    public string? Search { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public global::Sieve.Models.SieveModel? SieveModel { get; set; }
}

