using Application.ApiContracts.Output.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.Outputs.Queries.GetDeletedOutputsList;

public sealed record GetDeletedOutputsListQuery(SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<OutputResponse>>;
