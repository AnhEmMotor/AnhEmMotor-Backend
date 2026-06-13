using Application.ApiContracts.Evaluation.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.ServiceWorkshopEvaluations.Queries.GetServiceWorkshopEvaluations;

public class GetServiceWorkshopEvaluationsQuery : IRequest<Result<PagedResult<ServiceEvaluationListRowResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}

