using Application.ApiContracts.Output.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.Outputs.Queries.GetOutputsList;

public sealed record GetOutputsListQuery(SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<OutputResponse>>;
