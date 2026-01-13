using Application.ApiContracts.Output.Responses;
using MediatR;
using Sieve.Models;
using Domain.Primitives;
using Application.Common.Models;

namespace Application.Features.Outputs.Queries.GetOutputsByUserId;

public sealed record GetOutputsByUserIdQuery : IRequest<Result<PagedResult<OutputResponse>>>
{
    public Guid? BuyerId { get; init; }
    public SieveModel? SieveModel { get; init; }
}