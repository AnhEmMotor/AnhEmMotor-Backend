using Application.ApiContracts.Output.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.Outputs.Queries.GetOutputsByBuyerId;

public sealed record GetOutputsByUserIdByManagerQuery(Guid BuyerId, SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<OutputResponse>>;
