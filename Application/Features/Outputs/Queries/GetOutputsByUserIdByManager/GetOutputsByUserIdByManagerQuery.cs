using Application.ApiContracts.Output.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.Outputs.Queries.GetOutputsByUserIdByManager;

public sealed record GetOutputsByUserIdByManagerQuery(Guid BuyerId, SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<OutputResponse>>;
