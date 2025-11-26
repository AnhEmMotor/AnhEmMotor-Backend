using Application.ApiContracts.Output;
using Domain.Shared;
using MediatR;
using Sieve.Models;

namespace Application.Features.Outputs.Queries.GetOutputsList;

public sealed record GetOutputsListQuery(SieveModel SieveModel) : IRequest<PagedResult<OutputResponse>>;
