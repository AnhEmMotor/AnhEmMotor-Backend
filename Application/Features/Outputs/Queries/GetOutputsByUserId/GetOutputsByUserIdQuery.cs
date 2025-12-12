using Application.ApiContracts.Output.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.Outputs.Queries.GetOutputsByBuyerId;

public sealed record GetOutputsByUserIdQuery(Guid BuyerId, SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<OutputResponse>>;
