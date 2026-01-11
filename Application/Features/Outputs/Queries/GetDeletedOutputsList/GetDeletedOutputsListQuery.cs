using Application.ApiContracts.Output.Responses;
using MediatR;
using Sieve.Models;
using Domain.Primitives;
using Application.Common.Models;

namespace Application.Features.Outputs.Queries.GetDeletedOutputsList;

public sealed record GetDeletedOutputsListQuery(SieveModel SieveModel) : IRequest<Result<PagedResult<OutputResponse>>>;
