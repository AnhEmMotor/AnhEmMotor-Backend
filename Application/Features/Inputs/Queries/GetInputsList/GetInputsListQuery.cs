using Application.ApiContracts.Input.Responses;
using Domain.Shared;
using MediatR;
using Sieve.Models;

namespace Application.Features.Inputs.Queries.GetInputsList;

public sealed record GetInputsListQuery(SieveModel SieveModel) : IRequest<PagedResult<InputResponse>>;
