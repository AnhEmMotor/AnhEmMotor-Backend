using Application.ApiContracts.Output.Responses;
using Domain.Shared;
using MediatR;
using Sieve.Models;

namespace Application.Features.Outputs.Queries.GetDeletedOutputsList;

public sealed record GetDeletedOutputsListQuery(SieveModel SieveModel) : IRequest<PagedResult<OutputResponse>>;
