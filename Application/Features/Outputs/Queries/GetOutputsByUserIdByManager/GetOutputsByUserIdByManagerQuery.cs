using Application.ApiContracts.Output.Responses;
using MediatR;
using Sieve.Models;
using Domain.Primitives;
using Application.Common.Models;

namespace Application.Features.Outputs.Queries.GetOutputsByUserIdByManager;

public sealed record GetOutputsByUserIdByManagerQuery(Guid BuyerId, SieveModel SieveModel) : IRequest<Result<PagedResult<OutputResponse>>>;
