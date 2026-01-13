using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Outputs.Queries.GetOutputsByUserIdByManager;

public sealed record GetOutputsByUserIdByManagerQuery : IRequest<Result<PagedResult<OutputResponse>>>
{
    public Guid? BuyerId { get; init; }

    public SieveModel? SieveModel { get; init; }
}
