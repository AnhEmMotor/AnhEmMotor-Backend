using Application.ApiContracts.Input.Responses;
using MediatR;
using Domain.Primitives;
using Sieve.Models;
using Application.Common.Models;

namespace Application.Features.Inputs.Queries.GetDeletedInputsList;

public sealed record GetDeletedInputsListQuery: IRequest<Result<PagedResult<InputResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
