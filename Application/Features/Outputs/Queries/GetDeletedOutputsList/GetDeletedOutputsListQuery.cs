using Application.ApiContracts.Output.Responses;
using MediatR;
using Sieve.Models;
using Domain.Primitives;
using Application.Common.Models;

namespace Application.Features.Outputs.Queries.GetDeletedOutputsList;

public sealed record GetDeletedOutputsListQuery : IRequest<Result<PagedResult<OutputResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
