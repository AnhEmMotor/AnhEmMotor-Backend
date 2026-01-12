using Application.ApiContracts.Input.Responses;
using MediatR;
using Sieve.Models;
using Domain.Primitives;
using Application.Common.Models;

namespace Application.Features.Inputs.Queries.GetInputsList;

public sealed record GetInputsListQuery : IRequest<Result<PagedResult<InputResponse>>>
{
    public SieveModel? SieveModel { get; set; }
}