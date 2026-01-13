using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Outputs.Queries.GetDeletedOutputsList;

public sealed record GetDeletedOutputsListQuery : IRequest<Result<PagedResult<OutputResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
