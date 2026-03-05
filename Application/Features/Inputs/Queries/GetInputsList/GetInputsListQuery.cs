using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Inputs.Queries.GetInputsList;

public sealed record GetInputsListQuery : IRequest<Result<PagedResult<InputListResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}