using Application.ApiContracts.Input.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.Inputs.Queries.GetDeletedInputsList;

public sealed record GetDeletedInputsListQuery(SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<InputResponse>>;
