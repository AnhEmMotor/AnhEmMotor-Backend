using Application.ApiContracts.Input.Responses;
using Domain.Shared;
using MediatR;
using Sieve.Models;

namespace Application.Features.Inputs.Queries.GetDeletedInputsList;

public sealed record GetDeletedInputsListQuery(SieveModel SieveModel) : IRequest<PagedResult<InputResponse>>;
