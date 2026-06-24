using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Outputs.Queries.GetOutputsByUserIdForManager;

public record GetOutputsByUserIdForManagerQuery(Guid? BuyerId, SieveModel? SieveModel) : IRequest<Result<PagedResult<OutputItemResponse>>>;
