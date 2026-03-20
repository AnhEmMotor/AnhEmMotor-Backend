using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Outputs.Queries.GetOutputsByUserId;

public class GetOutputsByUserIdQuery : IRequest<Result<PagedResult<MyOrderResponse>>>
{
    public Guid? BuyerId { get; init; }

    public SieveModel? SieveModel { get; init; }
}