using Application.ApiContracts.Input.Responses;
using MediatR;
using Sieve.Models;
using Domain.Primitives;
using Application.Common.Models;

namespace Application.Features.Inputs.Queries.GetInputsList;

public sealed record GetInputsListQuery(SieveModel SieveModel) : IRequest<Result<PagedResult<InputResponse>>>;
