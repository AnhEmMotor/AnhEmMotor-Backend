using Application.ApiContracts.Output.Responses;
using MediatR;
using Sieve.Models;
using Domain.Primitives;
using Application.Common.Models;

namespace Application.Features.Outputs.Queries.GetOutputsByUserId;

public sealed record GetOutputsByUserIdQuery(Guid BuyerId, SieveModel SieveModel) : IRequest<Result<PagedResult<OutputResponse>>>;
