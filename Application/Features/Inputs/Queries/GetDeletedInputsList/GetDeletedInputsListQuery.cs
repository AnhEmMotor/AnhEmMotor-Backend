using Application.ApiContracts.Input.Responses;
using MediatR;
using Domain.Primitives;
using Sieve.Models;
using Application.Common.Models;

namespace Application.Features.Inputs.Queries.GetDeletedInputsList;

public sealed record GetDeletedInputsListQuery(SieveModel SieveModel) : IRequest<Result<PagedResult<InputResponse>>>;
