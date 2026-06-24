using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Outputs.Queries.GetOutputsForCurrentUser;

public record GetOutputsForCurrentUserQuery(SieveModel? SieveModel) : IRequest<Result<PagedResult<MyOrderResponse>>>;
