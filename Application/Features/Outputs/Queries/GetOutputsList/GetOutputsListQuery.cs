using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Outputs.Queries.GetOutputsList;

public sealed record GetOutputsListQuery : IRequest<Result<PagedResult<OutputResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
